using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Guids;

namespace Blogio.Blog
{
    public class BlogPostVersionTag : Entity<Guid>
    {
        public Guid BlogPostVersionId { get; set; }
        public BlogPostVersion Version { get; set; } = default!; 

        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = default!;

        public override object[] GetKeys() => new object[] { BlogPostVersionId, TagId };
    }
}
