using Blogio.Blazor.Components.Pages.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;         // ToListAsync için (istersen AsyncExecuter da kullanıyoruz)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;


namespace Blogio.Blog
{
    public class BlogAppService : ApplicationService, IBlogAppService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IRepository<Comment, Guid> _commentRepository;
        private readonly IRepository<Tag, Guid> _tagRepository;
        private readonly IRepository<BlogPostTag> _blogPostTagRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        public BlogAppService(
            IBlogRepository blogRepository,
            IRepository<Comment, Guid> commentRepository,
            IRepository<Tag, Guid> tagRepository,
            IRepository<BlogPostTag> blogPostTagRepository,
            IdentityUserManager userManager,
            IRepository<IdentityUser, Guid> userRepository)
        {
            _blogRepository = blogRepository;
            _commentRepository = commentRepository;
            _tagRepository = tagRepository;
            _blogPostTagRepository = blogPostTagRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        private bool CanManage =>
        CurrentUser.IsInRole("admin") || CurrentUser.IsInRole("author");


        // --------- BlogPost ---------
        public async Task<BlogPostDto> GetAsync(Guid id)
        {
            var query = await _blogRepository.WithDetailsAsync(); // repo’daki Include’ları içerir
            var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(p => p.Id == id));

            if (entity == null)
                throw new EntityNotFoundException(typeof(BlogPost), id);

            return ObjectMapper.Map<BlogPost, BlogPostDto>(entity);
        }

        public async Task<PagedResultDto<BlogPostListItemDto>> GetListAsync(GetBlogPostListDto input)
        {

            var query = (await _blogRepository.GetQueryableAsync());

            // Filters
            if (!input.Filter.IsNullOrWhiteSpace())
            {
                var f = input.Filter.Trim();
                query = query.Where(p => p.Title.Contains(f) || p.Content.Contains(f));
            }

            if (input.IsPublished.HasValue)
                query = query.Where(p => p.IsPublished == input.IsPublished.Value);

            if (input.AuthorId.HasValue)
                query = query.Where(p => p.AuthorId == input.AuthorId.Value);

            if (input.TagIds != null && input.TagIds.Count > 0)
                query = query.Where(p => p.BlogPostTags.Any(t => input.TagIds.Contains(t.TagId)));

            if (input.CreationTimeStart.HasValue)
                query = query.Where(p => p.CreationTime >= input.CreationTimeStart.Value);

            if (input.CreationTimeEnd.HasValue)
                query = query.Where(p => p.CreationTime <= input.CreationTimeEnd.Value);

            // Total count
            var totalCount = await AsyncExecuter.CountAsync(query);

            // Sorting (default: CreationTime desc)
            var sorting = input.Sorting.IsNullOrWhiteSpace() ? "CreationTime desc" : input.Sorting!;
            query = query.OrderBy(sorting);

            // Page
            var paged = query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                // Liste DTO’su için gerekli alanlar + CommentCount projeksiyonu
                .Select(p => new BlogPostListItemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    IsPublished = p.IsPublished,
                    AuthorId = p.AuthorId,
                    LikeCount = p.LikeCount,
                    CommentCount = p.Comments.Count,
                    CreationTime = p.CreationTime,
                    LastModificationTime = p.LastModificationTime
                });

            var items = await AsyncExecuter.ToListAsync(paged);
            return new PagedResultDto<BlogPostListItemDto>(totalCount, items);
        }

        [Authorize(Roles = "admin,author")]
        public async Task<BlogPostDto> CreateAsync(CreateUpdateBlogPostDto input)
        {
            // Tag existence check (EF bağımlılığı yok)
            if (input.TagIds?.Count > 0)
            {
                var existingTags = await _tagRepository.GetListAsync(x => input.TagIds.Contains(x.Id));
                var existingTagIds = existingTags.Select(x => x.Id).ToList();

                var missing = input.TagIds.Except(existingTagIds).ToList();
                if (missing.Count > 0)
                    throw new BusinessException("Blog:TagNotFound")
                        .WithData("TagIds", string.Join(",", missing));
            }

            var entity = ObjectMapper.Map<CreateUpdateBlogPostDto, BlogPost>(input);
            entity.IsPublished = input.IsPublished ?? false;
            entity.LikeCount = 0;

            entity = await _blogRepository.InsertAsync(entity, autoSave: true);

            if (input.TagIds != null && input.TagIds.Count > 0)
            {
                var joins = input.TagIds.Select(tagId => new BlogPostTag
                {
                    BlogPostId = entity.Id,
                    TagId = tagId
                }).ToList();

                await _blogPostTagRepository.InsertManyAsync(joins, autoSave: true);
            }

            return await GetAsync(entity.Id);
        }

        [Authorize(Roles = "admin,author")]
        public async Task<BlogPostDto> UpdateAsync(Guid id, CreateUpdateBlogPostDto input)
        {
            var entity = await _blogRepository.GetAsync(id);

            entity.Title = input.Title;
            entity.Content = input.Content;
            entity.AuthorId = input.AuthorId;
            if (input.IsPublished.HasValue)
                entity.IsPublished = input.IsPublished.Value;

            await _blogRepository.UpdateAsync(entity, autoSave: true);

            // Tag sync
            var newTagIds = input.TagIds ?? new List<Guid>();

            var currentJoins = await _blogPostTagRepository.GetListAsync(x => x.BlogPostId == id);
            var current = currentJoins.Select(x => x.TagId).ToList();

            var toAdd = newTagIds.Except(current).ToList();
            var toRemove = current.Except(newTagIds).ToList();

            if (toAdd.Count > 0)
            {
                var joins = toAdd.Select(tagId => new BlogPostTag
                {
                    BlogPostId = id,
                    TagId = tagId
                }).ToList();

                await _blogPostTagRepository.InsertManyAsync(joins, autoSave: true);
            }

            if (toRemove.Count > 0)
            {
                await _blogPostTagRepository.DeleteAsync(x => x.BlogPostId == id && toRemove.Contains(x.TagId));
            }

            return await GetAsync(id);
        }

        [Authorize(Roles = "admin,author")]
        public async Task DeleteAsync(Guid id)
        {
            // (İsteğe bağlı) Join ve yorumları manuel silmek istersen:
            await _blogPostTagRepository.DeleteAsync(x => x.BlogPostId == id);
            await _commentRepository.DeleteAsync(x => x.BlogPostId == id);

            await _blogRepository.DeleteAsync(id);
        }

        [Authorize(Roles = "admin,author")]
        public async Task PublishAsync(Guid id)
        {
            var entity = await _blogRepository.GetAsync(id);
            if (!entity.IsPublished)
            {
                entity.IsPublished = true;
                await _blogRepository.UpdateAsync(entity, autoSave: true);
            }
        }

        [Authorize(Roles = "admin,author")]
        public async Task UnpublishAsync(Guid id)
        {
            var entity = await _blogRepository.GetAsync(id);
            if (entity.IsPublished)
            {
                entity.IsPublished = false;
                await _blogRepository.UpdateAsync(entity, autoSave: true);
            }
        }

        public async Task<int> LikeAsync(Guid id)
        {
            var entity = await _blogRepository.GetAsync(id);
            entity.LikeCount++;
            await _blogRepository.UpdateAsync(entity, autoSave: true);
            return entity.LikeCount;
        }

        public async Task<int> UnlikeAsync(Guid id)
        {
            var entity = await _blogRepository.GetAsync(id);
            if (entity.LikeCount > 0)
            {
                entity.LikeCount--;
                await _blogRepository.UpdateAsync(entity, autoSave: true);
            }
            return entity.LikeCount;
        }



        // -------------------- Comments --------------------

        public async Task<ListResultDto<CommentDto>> GetCommentsAsync(Guid blogPostId)
        {
            // 1) IQueryable al
            var queryable = await _commentRepository.GetQueryableAsync();

            // 2) Filtre + sıralama
            var query = queryable
                .Where(c => c.BlogPostId == blogPostId)
                .OrderBy(c => c.CreationTime);

            // 3) Server-side execute
            var list = await AsyncExecuter.ToListAsync(query);

            // 4) Map
            var dtos = ObjectMapper.Map<List<Comment>, List<CommentDto>>(list);

            // 5) Yorum sahiplerinin user adlarını doldur
            var userIds = dtos.Where(d => d.CreatorId.HasValue)
                              .Select(d => d.CreatorId!.Value)
                              .Distinct()
                              .ToList();

            if (userIds.Count > 0)
            {
                // UserManager.Users yerine repository kullan
                var users = await _userRepository.GetListAsync(u => userIds.Contains(u.Id));
                var dict = users.ToDictionary(u => u.Id, u => u.UserName);

                foreach (var d in dtos)
                    if (d.CreatorId.HasValue && dict.TryGetValue(d.CreatorId.Value, out var uname))
                        d.CreatorUserName = uname;
            }

            return new ListResultDto<CommentDto>(dtos);
        }


        [Authorize]
        public async Task<CommentDto> AddCommentAsync(CreateUpdateCommentDto input)
        {
            var exists = await _blogRepository.AnyAsync(p => p.Id == input.BlogPostId);
            if (!exists)
                throw new EntityNotFoundException(typeof(BlogPost), input.BlogPostId);

            // Sadece giriş kontrolü – CreatorId ABP tarafından otomatik set edilecek
            _ = CurrentUser.Id ?? throw new AbpAuthorizationException("You must login to add a comment.");

            var comment = new Comment(
                GuidGenerator.Create(),
                input.Text,            // <- ÖNCE text
                input.BlogPostId       // <- sonra blogPostId
            );

            await _commentRepository.InsertAsync(comment, autoSave: true);

            var dto = ObjectMapper.Map<Comment, CommentDto>(comment);
            dto.CreatorUserName = CurrentUser.UserName; // ekrana hızlı yansıtmak için
            return dto;
        }


        // -------------------- Tags --------------------

        public async Task<ListResultDto<TagDto>> GetAllTagsAsync()
        {
            var tags = await _tagRepository.GetListAsync();
            return new ListResultDto<TagDto>(
                ObjectMapper.Map<List<Tag>, List<TagDto>>(tags));
        }

        public async Task<TagDto> CreateTagAsync(CreateUpdateTagDto input)
        {
            // unique name kontrolü (opsiyonel)
            var exists = await _tagRepository.AnyAsync(t => t.Name == input.Name);
            if (exists)
                throw new BusinessException("Blog:TagAlreadyExists").WithData("Name", input.Name);

            var tag = new Tag(GuidGenerator.Create(), input.Name);

            tag = await _tagRepository.InsertAsync(tag, autoSave: true);
            return ObjectMapper.Map<Tag, TagDto>(tag);
        }

        [Authorize(Roles = "admin,author")]
        public async Task<TagDto> UpdateTagAsync(Guid id, CreateUpdateTagDto input)
        {
            var tag = await _tagRepository.GetAsync(id);

            // unique name kontrolü (opsiyonel)
            var exists = await _tagRepository.AnyAsync(t => t.Id != id && t.Name == input.Name);
            if (exists)
                throw new BusinessException("Blog:TagAlreadyExists").WithData("Name", input.Name);

            tag.Name = input.Name;
            await _tagRepository.UpdateAsync(tag, autoSave: true);
            return ObjectMapper.Map<Tag, TagDto>(tag);
        }

        [Authorize(Roles = "admin,author")]
        public async Task DeleteTagAsync(Guid id)
        {
            // joinleri temizle
            await _blogPostTagRepository.DeleteAsync(x => x.TagId == id);
            await _tagRepository.DeleteAsync(id);
        }


        // -------------------- Authors --------------------

        public async Task<ListResultDto<AuthorDto>> GetAuthorsAsync()
        {
            // 1) "author" rolündeki kullanıcıları çek
            var users = await _userManager.GetUsersInRoleAsync("author");

            // 2) Post sayıları
            var ids = users.Select(u => u.Id).ToList();
            var q = await _blogRepository.GetQueryableAsync();
            var counts = await AsyncExecuter.ToListAsync(
                q.Where(p => ids.Contains(p.AuthorId))
                 .GroupBy(p => p.AuthorId)
                 .Select(g => new { AuthorId = g.Key, Count = g.Count() })
            );
            var dict = counts.ToDictionary(x => x.AuthorId, x => x.Count);

            // 3) DTO
            var items = users.Select(u => new AuthorDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Name = u.Name,
                Surname = u.Surname,
                Email = u.Email,
                PostCount = dict.TryGetValue(u.Id, out var c) ? c : 0
            }).ToList();

            return new ListResultDto<AuthorDto>(items);
        }

        public Task DeleteCommentAsync(Guid commentId)
        {
            throw new NotImplementedException();
        }
    }
}
