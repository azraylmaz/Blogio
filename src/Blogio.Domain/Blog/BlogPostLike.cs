using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Blogio.Blog
{
    public class BlogPostLike : Entity<Guid>
    {
        public Guid BlogPostId { get; private set; }
        public Guid UserId { get; private set; }

        protected BlogPostLike() { }

        public BlogPostLike(Guid id, Guid blogPostId, Guid userId) : base(id)
        {
            BlogPostId = blogPostId;
            UserId = userId;
        }
    }
}
