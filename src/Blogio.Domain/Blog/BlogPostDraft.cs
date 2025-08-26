using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Blogio.Blog
{
    public class BlogPostDraft: AuditedAggregateRoot<Guid>
    {
        public Guid BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; } = default!;   // <-- mapping'de .HasOne(d => d.BlogPost) burayı bekliyor

        public Guid OwnerUserId { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = default!;
        public bool IsActive { get; set; }

        public ICollection<BlogPostDraftTag> Tags { get; set; } = new List<BlogPostDraftTag>();
    }
}
