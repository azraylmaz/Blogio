using Blogio.Blog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Blogio.Blog
{
    public class Comment : AuditedAggregateRoot<Guid>
    {
        public string Text { get; set; }
        public Guid BlogPostId { get; set; }

        // Navigation property (BlogPost ile ilişki)
        public BlogPost BlogPost { get; set; }

        protected Comment() { }

        public Comment(Guid id, string text, Guid blogPostId)
            : base(id)
        {
            Text = text;
            BlogPostId = blogPostId;
        }
    }
}
