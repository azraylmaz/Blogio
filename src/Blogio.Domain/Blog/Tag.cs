
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace Blogio.Blog
{
    public class Tag : Entity<Guid>
    {
        public string Name { get; set; }

        // Many-to-many
        public ICollection<BlogPostTag> BlogPostTags { get; set; }

        public Tag() { }

        public Tag(Guid id, string name) : base(id)
        {
            Name = name;
            BlogPostTags = new List<BlogPostTag>();
        }
    }
}
