using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Blogio.Blog
{
    public class BlogPostDto :FullAuditedEntityDto<Guid>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPublished { get; set; }
        public Guid AuthorId { get; set; }
        public int LikeCount { get; set; }
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public List<TagDto> BlogPostTags { get; set; } = new List<TagDto>();
    }
}
