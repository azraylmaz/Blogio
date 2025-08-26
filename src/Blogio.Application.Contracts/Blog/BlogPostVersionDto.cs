using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Blogio.Blog
{
    public class BlogPostVersionDto : EntityDto<Guid>
    {
        public Guid BlogPostId { get; set; }
        public int Version { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreationTime { get; set; }

        public List<TagDto> Tags { get; set; } = new();
    }
}
