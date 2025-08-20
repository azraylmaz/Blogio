using Blogio.Blog;
using System;
using Volo.Abp.Domain.Entities;

namespace Blogio.Blog
{
    // Composite key: (BlogPostId, TagId)
    public class BlogPostTag : Entity
    {
        public Guid BlogPostId { get; set; }
        public Guid TagId { get; set; }

        // Navigation (opsiyonel ama faydalı)
        public BlogPost BlogPost { get; set; }
        public Tag Tag { get; set; }

        public BlogPostTag() { }

        public BlogPostTag(Guid blogPostId, Guid tagId)
        {
            BlogPostId = blogPostId;
            TagId = tagId;
        }

        public override object[] GetKeys() => new object[] { BlogPostId, TagId };
    }
}
