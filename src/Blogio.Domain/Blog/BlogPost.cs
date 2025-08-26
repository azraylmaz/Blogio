using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace Blogio.Blog
{
    public class BlogPost : FullAuditedAggregateRoot<Guid>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsPublished { get; set; }
        public Guid AuthorId { get; set; }
        public int LikeCount { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<BlogPostTag> BlogPostTags { get; set; }

        public ICollection<BlogPostDraft> Drafts { get; set; } = new List<BlogPostDraft>();
        public ICollection<BlogPostVersion> Versions { get; set; } = new List<BlogPostVersion>();

        protected BlogPost() { }

        public BlogPost(Guid id, string title, string content, Guid authorId)
            : base(id)
        {
            Title = title;
            Content = content;
            AuthorId = authorId;
            LikeCount = 0;
            Comments = new List<Comment>();
            BlogPostTags = new List<BlogPostTag>();
            Drafts = new List<BlogPostDraft>();
            Versions = new List<BlogPostVersion>();
        }
    }
}
