
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


        // -------- Likes ---------
        Task<int> LikeAsync(Guid id);
        Task<int> UnlikeAsync(Guid id);
        Task<bool> IsLikedByMeAsync(Guid id);



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

        // ----- Draft approval --------
        Task SubmitDraftForApprovalAsync(Guid blogPostId);                // yazar: onaya gönder
        Task<ListResultDto<BlogPostDraftDto>> GetPendingDraftsAsync();    // admin: bekleyenler
        Task ApproveDraftAsync(Guid draftId);                              // admin: onayla
        Task RejectDraftAsync(Guid draftId, string? note);

        // DRAFT
        Task<BlogPostDraftDto?> GetDraftAsync(Guid blogPostId);                           // aktif taslağım
        Task<BlogPostDraftDto> UpsertDraftAsync(CreateUpdateBlogPostDraftDto input);      // oluştur/güncelle
        Task DeleteDraftAsync(Guid blogPostId);                                           // taslağı sil


        // VERSIONS
        Task<ListResultDto<BlogPostVersionDto>> GetVersionsAsync(Guid blogPostId, int maxCount = 10);
        Task<BlogPostVersionDto?> GetVersionAsync(Guid versionId);
        Task RevertToVersionAsync(Guid versionId);

    }
}
