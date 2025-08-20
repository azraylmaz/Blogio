using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Blogio.Blog
{
    public class BlogPostListItemDto : AuditedEntityDto<Guid>
    {
        public string Title { get; set; }
        public bool IsPublished { get; set; }
        public Guid AuthorId { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
    }
}
