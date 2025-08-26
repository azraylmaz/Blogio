using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Blogio.Blog
{
    public class BlogPostVersion : AuditedAggregateRoot<Guid>
    {
        public Guid BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = default!;     // <-- .HasOne(v => v.BlogPost)

        public int Version { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;

        public ICollection<BlogPostVersionTag> BlogPostVersionTags { get; set; } = new List<BlogPostVersionTag>();
    }
}
