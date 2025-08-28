
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;         // ToListAsync için (istersen AsyncExecuter da kullanıyoruz)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;


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
        private readonly IRepository<BlogPostLike, Guid> _likeRepository;
        private readonly IRepository<BlogPostDraft, Guid> _draftRepository;
        private readonly IRepository<BlogPostDraftTag, Guid> _draftTagRepository;
        private readonly IRepository<BlogPostVersion, Guid> _versionRepository;
        private readonly IRepository<BlogPostVersionTag, Guid> _versionTagRepository;


        public BlogAppService(
            IBlogRepository blogRepository,
            IRepository<Comment, Guid> commentRepository,
            IRepository<Tag, Guid> tagRepository,
            IRepository<BlogPostTag> blogPostTagRepository,
            IdentityUserManager userManager,
            IRepository<IdentityUser, Guid> userRepository,
            IRepository<BlogPostLike, Guid> likeRepository,
            IRepository<BlogPostDraft, Guid> draftRepository,          
            IRepository<BlogPostDraftTag, Guid> draftTagRepository,    
            IRepository<BlogPostVersion, Guid> versionRepository,      
            IRepository<BlogPostVersionTag, Guid> versionTagRepository)
        {
            _blogRepository = blogRepository;
            _commentRepository = commentRepository;
            _tagRepository = tagRepository;
            _blogPostTagRepository = blogPostTagRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _likeRepository = likeRepository;
            _draftRepository = draftRepository;            
            _draftTagRepository = draftTagRepository;      
            _versionRepository = versionRepository;        
            _versionTagRepository = versionTagRepository;
        }

        private Guid? Me => CurrentUser.Id;
        private bool IsAdmin => CurrentUser.IsInRole("admin");
        private bool IsAuthor => CurrentUser.IsInRole("author");

        private bool CanManagePost(BlogPost p)
            => IsAdmin || (IsAuthor && Me.HasValue && p.AuthorId == Me.Value);

        // --------- BlogPost ---------
        public async Task<BlogPostDto> GetAsync(Guid id)
        {
            var q = await _blogRepository.GetQueryableAsync();

            var entity = await AsyncExecuter.FirstOrDefaultAsync(
                q.Where(p => p.Id == id)
                 .Include(p => p.BlogPostTags)
                    .ThenInclude(j => j.Tag)   // <- ÖNEMLİ: Tag adları için
            );

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

            if (!IsAdmin)
            {
                var myId = Me ?? Guid.Empty;
                // Herkese ait published + bana ait draft/published
                query = query.Where(p => p.IsPublished || p.AuthorId == myId);
            }

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
            // Author (admin değil) ise AuthorId’yi kendisine zorla
            var me = CurrentUser.Id;
            var admin = CurrentUser.IsInRole("admin");
            if (me.HasValue && !admin)
                input.AuthorId = me.Value;

            if (!IsAdmin)
                input.AuthorId = Me ?? throw new AbpAuthorizationException();

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

            var entity = new BlogPost(
                    GuidGenerator.Create(),
                    input.Title,
                    input.Content ?? string.Empty,
                    input.AuthorId
                )
            {
                IsPublished = input.IsPublished ?? false,
                LikeCount = 0
            };

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
            if (!CanManagePost(entity)) throw new AbpAuthorizationException();

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
            var entity = await _blogRepository.GetAsync(id);
            if (!CanManagePost(entity)) throw new AbpAuthorizationException();

            // (İsteğe bağlı) Join ve yorumları manuel silmek istersen:
            await _blogPostTagRepository.DeleteAsync(x => x.BlogPostId == id);
            await _commentRepository.DeleteAsync(x => x.BlogPostId == id);

            await _blogRepository.DeleteAsync(id);
        }

        [Authorize(Roles = "admin,author")]
        public async Task PublishAsync(Guid id)
        {
            var entity = await _blogRepository.GetAsync(id);
            if (!CanManagePost(entity)) throw new AbpAuthorizationException();
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
            if (!CanManagePost(entity)) throw new AbpAuthorizationException();
            if (entity.IsPublished)
            {
                entity.IsPublished = false;
                await _blogRepository.UpdateAsync(entity, autoSave: true);
            }
        }

        // ---------- Likes -------------

        [Authorize]
        public async Task<int> LikeAsync(Guid id)
        {
            var me = CurrentUser.Id ?? throw new AbpAuthorizationException();

            // daha önce beğenmişse ekleme
            var exists = await _likeRepository.AnyAsync(x => x.BlogPostId == id && x.UserId == me);
            if (!exists)
            {
                await _likeRepository.InsertAsync(
                    new BlogPostLike(GuidGenerator.Create(), id, me),
                    autoSave: true
                );
            }

            // sayıyı yeniden hesapla ve aggregate'ı eşitle
            var count = await _likeRepository.CountAsync(x => x.BlogPostId == id);
            var post = await _blogRepository.GetAsync(id);
            if (post.LikeCount != count)
            {
                post.LikeCount = count;
                await _blogRepository.UpdateAsync(post, autoSave: true);
            }

            return count;
        }

        [Authorize]
        public async Task<int> UnlikeAsync(Guid id)
        {
            var me = CurrentUser.Id ?? throw new AbpAuthorizationException();

            await _likeRepository.DeleteAsync(x => x.BlogPostId == id && x.UserId == me);

            // >>> Silmeyi DB’ye yaz
            await CurrentUnitOfWork.SaveChangesAsync();

            // >>> Artık doğru sayıyı okur
            var count = await _likeRepository.CountAsync(x => x.BlogPostId == id);

            var post = await _blogRepository.GetAsync(id);
            if (post.LikeCount != count)
            {
                post.LikeCount = count;
                await _blogRepository.UpdateAsync(post, autoSave: true);
            }

            return count;
        }

        [AllowAnonymous] // anonim erişime izin ver
        [HttpGet("is-liked-by-me")]
        public async Task<bool> IsLikedByMeAsync(Guid id)
        {
            var me = CurrentUser.Id;
            if (!me.HasValue) return false; // login değilse false
            return await _likeRepository.AnyAsync(x => x.BlogPostId == id && x.UserId == me.Value);
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

        [Authorize(Roles = "admin")]
        public async Task DeleteCommentAsync(Guid commentId)
        {
            await _commentRepository.DeleteAsync(commentId);
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

        // -------------------- Draft --------------------
        public async Task<BlogPostDraftDto?> GetDraftAsync(Guid blogPostId)
        {
            // login değilse taslak döndürmeyelim
            if (!Me.HasValue) return null;

            var q = (await _draftRepository.GetQueryableAsync())
                .Where(d => d.BlogPostId == blogPostId && d.OwnerUserId == Me.Value && d.IsActive)
                .Include(d => d.Tags).ThenInclude(t => t.Tag);

            var draft = await AsyncExecuter.FirstOrDefaultAsync(q);
            return draft is null ? null : ObjectMapper.Map<BlogPostDraft, BlogPostDraftDto>(draft);
        }

        public async Task<BlogPostDraftDto> UpsertDraftAsync(CreateUpdateBlogPostDraftDto input)
        {
            var post = await _blogRepository.GetAsync(input.BlogPostId);
            if (!CanManagePost(post)) throw new AbpAuthorizationException();

            var uid = Me ?? throw new AbpAuthorizationException();

            // aktif taslağım var mı?
            var q = (await _draftRepository.GetQueryableAsync())
                .Where(d => d.BlogPostId == input.BlogPostId && d.OwnerUserId == uid && d.IsActive)
                .Include(d => d.Tags);
            var draft = await AsyncExecuter.FirstOrDefaultAsync(q);

            if (draft is null)
            {
                draft = new BlogPostDraft
                {                    
                    BlogPostId = input.BlogPostId,
                    OwnerUserId = uid,
                    Title = input.Title,
                    Content = input.Content,
                    IsActive = true,
                    Status = DraftStatus.Editing
                };
                draft = await _draftRepository.InsertAsync(draft, autoSave: true);
            }
            else
            {
                // Pending ise kilitli
                if (draft.Status == DraftStatus.Submitted)
                    throw new BusinessException("Draft.PendingApproval")
                        .WithData("Message", "Draft is waiting for admin approval.");

                draft.Title = input.Title;
                draft.Content = input.Content;

                // Reddedilmişse tekrar düzenlemeye başladı => Editing'e çek
                if (draft.Status == DraftStatus.Rejected)
                    draft.Status = DraftStatus.Editing;

                await _draftRepository.UpdateAsync(draft, autoSave: true);
            }

            // Tag eşitle (tamamen yenile)
            await _draftTagRepository.DeleteAsync(x => x.BlogPostDraftId == draft.Id);
            if (input.TagIds?.Count > 0)
            {
                foreach (var tid in input.TagIds.Distinct())
                {
                    await _draftTagRepository.InsertAsync(
                        new BlogPostDraftTag
                        {
                            BlogPostDraftId = draft.Id,
                            TagId = tid
                        },
                        autoSave: false);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            // Tag -> TagDto için include ile geri oku
            var reloaded = await (await _draftRepository.GetQueryableAsync())
                .Where(d => d.Id == draft.Id)
                .Include(d => d.Tags).ThenInclude(t => t.Tag)
                .FirstAsync();

            return ObjectMapper.Map<BlogPostDraft, BlogPostDraftDto>(reloaded);
        }

        [Authorize(Roles = "admin,author")]
        public async Task SubmitDraftForApprovalAsync(Guid blogPostId)
        {
            var post = await _blogRepository.GetAsync(blogPostId);
            if (!CanManagePost(post)) throw new AbpAuthorizationException();

            var uid = Me ?? throw new AbpAuthorizationException();

            var draft = await (await _draftRepository.GetQueryableAsync())
                .Where(d => d.BlogPostId == blogPostId && d.OwnerUserId == uid && d.IsActive)
                .FirstOrDefaultAsync();

            if (draft is null)
                throw new BusinessException("DraftNotFound");

            if (draft.Status == DraftStatus.Submitted)
                return; // zaten beklemede

            draft.Status = DraftStatus.Submitted;
            await _draftRepository.UpdateAsync(draft, autoSave: true);
        }

        [Authorize(Roles = "admin")]
        public async Task<ListResultDto<BlogPostDraftDto>> GetPendingDraftsAsync()
        {
            var q = (await _draftRepository.GetQueryableAsync())
                .Where(d => d.IsActive && d.Status == DraftStatus.Submitted)
                .Include(d => d.Tags).ThenInclude(t => t.Tag);

            var list = await AsyncExecuter.ToListAsync(q);
            var dtos = ObjectMapper.Map<List<BlogPostDraft>, List<BlogPostDraftDto>>(list);
            return new ListResultDto<BlogPostDraftDto>(dtos);
        }


        public async Task DeleteDraftAsync(Guid blogPostId)
        {
            var uid = Me ?? throw new AbpAuthorizationException();
            var draft = await _draftRepository.FirstOrDefaultAsync(
                d => d.BlogPostId == blogPostId && d.OwnerUserId == uid && d.IsActive);
            if (draft is null) return;

            await _draftRepository.DeleteAsync(draft);
        }

        
        [Authorize(Roles = "admin")]
        public async Task ApproveDraftAsync(Guid draftId)
        {
            var draft = await (await _draftRepository.GetQueryableAsync())
                .Where(d => d.Id == draftId)
                .Include(d => d.Tags)
                .FirstOrDefaultAsync();

            if (draft is null || !draft.IsActive || draft.Status != DraftStatus.Submitted)
                throw new BusinessException("DraftNotInSubmittedState");

            var post = await _blogRepository.GetAsync(draft.BlogPostId);

            // 1) Şu anki canlı içeriği Version olarak arşivle (sende zaten vardı)
            var lastNo = await (await _versionRepository.GetQueryableAsync())
                .Where(v => v.BlogPostId == post.Id)
                .MaxAsync(v => (int?)v.Version) ?? 0;

            var version = new BlogPostVersion
            {
                BlogPostId = post.Id,
                Version = lastNo + 1,
                Title = post.Title,
                Content = post.Content
            };
            await _versionRepository.InsertAsync(version, autoSave: true);

            var currentPostTags = await _blogPostTagRepository.GetListAsync(x => x.BlogPostId == post.Id);
            foreach (var link in currentPostTags)
                await _versionTagRepository.InsertAsync(new BlogPostVersionTag
                {
                    BlogPostVersionId = version.Id,
                    TagId = link.TagId
                }, autoSave: false);
            await CurrentUnitOfWork.SaveChangesAsync();

            // 2) Post’u taslakla güncelle
            post.Title = draft.Title;
            post.Content = draft.Content;
            post.IsPublished = true;
            await _blogRepository.UpdateAsync(post, autoSave: true);

            // 3) Tag sync
            var draftTagIds = draft.Tags.Select(t => t.TagId).Distinct().ToList();
            var postTagIds = currentPostTags.Select(x => x.TagId).ToList();
            var toAdd = draftTagIds.Except(postTagIds).ToList();
            var toRemove = postTagIds.Except(draftTagIds).ToList();

            if (toRemove.Count > 0)
                await _blogPostTagRepository.DeleteAsync(x => x.BlogPostId == post.Id && toRemove.Contains(x.TagId));

            foreach (var tid in toAdd)
                await _blogPostTagRepository.InsertAsync(new BlogPostTag(post.Id, tid), autoSave: false);

            await CurrentUnitOfWork.SaveChangesAsync();

            // 4) Taslağı pasifle/işaretle
            draft.Status = DraftStatus.Approved;
            draft.IsActive = false;
            await _draftRepository.UpdateAsync(draft, autoSave: true);
        }

        [Authorize(Roles = "admin")]
        public async Task RejectDraftAsync(Guid draftId, string? note)
        {
            var draft = await _draftRepository.GetAsync(draftId);
            if (draft.Status != DraftStatus.Submitted)
                throw new BusinessException("DraftNotInSubmittedState");

            draft.Status = DraftStatus.Rejected;      // yazar tekrar düzenleyebilir
            draft.ReviewerNote = note;
            await _draftRepository.UpdateAsync(draft, autoSave: true);
        }



        // ---------------- Version ----------------
        public async Task<ListResultDto<BlogPostVersionDto>> GetVersionsAsync(Guid blogPostId, int maxCount = 10)
        {
            var q = (await _versionRepository.GetQueryableAsync())
                .Where(v => v.BlogPostId == blogPostId)
                .Include(v => v.BlogPostVersionTags).ThenInclude(t => t.Tag)
                .OrderByDescending(v => v.Version)
                .Take(maxCount);

            var list = await AsyncExecuter.ToListAsync(q);
            var dtos = ObjectMapper.Map<List<BlogPostVersion>, List<BlogPostVersionDto>>(list);
            return new ListResultDto<BlogPostVersionDto>(dtos);
        }

        public async Task<BlogPostVersionDto?> GetVersionAsync(Guid versionId)
        {
            var v = await (await _versionRepository.GetQueryableAsync())
                .Where(x => x.Id == versionId)
                .Include(x => x.BlogPostVersionTags).ThenInclude(t => t.Tag)
                .FirstOrDefaultAsync();

            return v is null ? null : ObjectMapper.Map<BlogPostVersion, BlogPostVersionDto>(v);
        }

        public async Task RevertToVersionAsync(Guid versionId)
        {
            var v = await (await _versionRepository.GetQueryableAsync())
                .Where(x => x.Id == versionId)
                .Include(x => x.BlogPostVersionTags)
                .FirstOrDefaultAsync();

            if (v is null) throw new BusinessException("VersionNotFound");

            var post = await _blogRepository.GetAsync(v.BlogPostId);
            if (!CanManagePost(post)) throw new AbpAuthorizationException();

            // Önce mevcut durumu yeni bir version olarak arşivle
            var lastNo = await (await _versionRepository.GetQueryableAsync())
                .Where(x => x.BlogPostId == post.Id)
                .MaxAsync(x => (int?)x.Version) ?? 0;

            var cur = new BlogPostVersion
            {
                BlogPostId = post.Id,
                Version = lastNo + 1,
                Title = post.Title,
                Content = post.Content
            };
            await _versionRepository.InsertAsync(cur, autoSave: true);

            var nowPostTags = await _blogPostTagRepository.GetListAsync(x => x.BlogPostId == post.Id);
            foreach (var link in nowPostTags)
            {
                await _versionTagRepository.InsertAsync(new BlogPostVersionTag
                {
                    BlogPostVersionId = cur.Id,
                    TagId = link.TagId
                }, autoSave: false);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            // Post'u seçili sürüme al
            post.Title = v.Title;
            post.Content = v.Content;
            await _blogRepository.UpdateAsync(post, autoSave: true);

            // Tagleri version'dan kopyala
            var versionTagIds = v.BlogPostVersionTags.Select(t => t.TagId).ToList();
            await _blogPostTagRepository.DeleteAsync(x => x.BlogPostId == post.Id); // temizle

            foreach (var tid in versionTagIds.Distinct())
                await _blogPostTagRepository.InsertAsync(new BlogPostTag(post.Id, tid), autoSave: false);

            await CurrentUnitOfWork.SaveChangesAsync();
        }


    }
}
