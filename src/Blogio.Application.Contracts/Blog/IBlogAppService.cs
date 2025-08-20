using Blogio.Blazor.Components.Pages.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Blogio.Blog
{
    public interface IBlogAppService : IApplicationService
    {
        // --- BlogPost CRUD & Queries ---
        Task<BlogPostDto> GetAsync(Guid id);
        Task<PagedResultDto<BlogPostListItemDto>> GetListAsync(GetBlogPostListDto input);
        Task<BlogPostDto> CreateAsync(CreateUpdateBlogPostDto input);
        Task<BlogPostDto> UpdateAsync(Guid id, CreateUpdateBlogPostDto input);
        Task DeleteAsync(Guid id);


        // --- State & Counters ---
        Task PublishAsync(Guid id);
        Task UnpublishAsync(Guid id);
        /// <summary>Beğeni sayısını 1 artırır ve yeni sayıyı döner.</summary>
        Task<int> LikeAsync(Guid id);
        /// <summary>Beğeni sayısını 1 azaltır (0’ın altına düşürme) ve yeni sayıyı döner.</summary>
        Task<int> UnlikeAsync(Guid id);


        // --- Comments ---
        Task<ListResultDto<CommentDto>> GetCommentsAsync(Guid blogPostId);
        Task<CommentDto> AddCommentAsync(CreateUpdateCommentDto input);
        Task DeleteCommentAsync(Guid commentId);


        // --- Tags ---
        Task<ListResultDto<TagDto>> GetAllTagsAsync();
        Task<TagDto> CreateTagAsync(CreateUpdateTagDto input);
        Task<TagDto> UpdateTagAsync(Guid id, CreateUpdateTagDto input);
        Task DeleteTagAsync(Guid id);


        // --- Authors ---
        Task<ListResultDto<AuthorDto>> GetAuthorsAsync();

    }
}
