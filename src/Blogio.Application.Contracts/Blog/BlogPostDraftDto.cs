using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Blogio.Blog
{
    public class BlogPostDraftDto : EntityDto<Guid>
    {
        public Guid BlogPostId { get; set; }
        public Guid OwnerUserId { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime CreationTime { get; set; }

        public List<TagDto> Tags { get; set; } = new();
    }

    public class CreateUpdateBlogPostDraftDto
    {
        public Guid BlogPostId { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public List<Guid> TagIds { get; set; } = new();
    }
}
