using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Guids;

namespace Blogio.Blog
{
    public class BlogPostDraftTag : Entity<Guid>
    {
        public Guid BlogPostDraftId { get; set; }
        public BlogPostDraft Draft { get; set; } = default!;  

        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = default!;

        public override object[] GetKeys() => new object[] { BlogPostDraftId, TagId };
    }
}
