using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Blogio.Blog
{
    [ApiController]
    [Route("api/blog")]
    public class BlogController : AbpController, IBlogAppService
    {
        private readonly IBlogAppService _blogAppService;

        public BlogController(IBlogAppService blogAppService)
        {
            _blogAppService = blogAppService;
        }


        // --------- BlogPost ---------

        [HttpGet("{id}")]
        public Task<BlogPostDto> GetAsync(Guid id)
            => _blogAppService.GetAsync(id);

        [HttpGet]
        public Task<PagedResultDto<BlogPostListItemDto>> GetListAsync([FromQuery] GetBlogPostListDto input)
            => _blogAppService.GetListAsync(input);

        [HttpPost]
        public Task<BlogPostDto> CreateAsync([FromBody] CreateUpdateBlogPostDto input)
            => _blogAppService.CreateAsync(input);

        [HttpPut("{id}")]
        public Task<BlogPostDto> UpdateAsync(Guid id, [FromBody] CreateUpdateBlogPostDto input)
            => _blogAppService.UpdateAsync(id, input);

        [HttpDelete("{id}")]
        public Task DeleteAsync(Guid id)
            => _blogAppService.DeleteAsync(id);

        [HttpPost("{id}/publish")]
        public Task PublishAsync(Guid id)
            => _blogAppService.PublishAsync(id);

        [HttpPost("{id}/unpublish")]
        public Task UnpublishAsync(Guid id)
            => _blogAppService.UnpublishAsync(id);

        [HttpPost("{id}/like")]
        public Task<int> LikeAsync(Guid id)
            => _blogAppService.LikeAsync(id);

        [HttpPost("{id}/unlike")]
        public Task<int> UnlikeAsync(Guid id)
            => _blogAppService.UnlikeAsync(id);

        // --------- Comments ---------

        [HttpGet("{blogPostId}/comments")]
        public Task<ListResultDto<CommentDto>> GetCommentsAsync(Guid blogPostId)
            => _blogAppService.GetCommentsAsync(blogPostId);

        [HttpPost("comments")]
        public Task<CommentDto> AddCommentAsync([FromBody] CreateUpdateCommentDto input)
            => _blogAppService.AddCommentAsync(input);

        [HttpDelete("comments/{commentId}")]
        public Task DeleteCommentAsync(Guid commentId)
            => _blogAppService.DeleteCommentAsync(commentId);

        // --------- Tags ---------

        [HttpGet("tags")]
        public Task<ListResultDto<TagDto>> GetAllTagsAsync()
            => _blogAppService.GetAllTagsAsync();

        [HttpPost("tags")]
        public Task<TagDto> CreateTagAsync([FromBody] CreateUpdateTagDto input)
            => _blogAppService.CreateTagAsync(input);

        [HttpPut("tags/{id}")]
        public Task<TagDto> UpdateTagAsync(Guid id, [FromBody] CreateUpdateTagDto input)
            => _blogAppService.UpdateTagAsync(id, input);

        [HttpDelete("tags/{id}")]
        public Task DeleteTagAsync(Guid id)
            => _blogAppService.DeleteTagAsync(id);
    }
}
