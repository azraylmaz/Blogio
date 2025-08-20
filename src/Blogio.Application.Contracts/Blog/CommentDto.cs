using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Blogio.Blog
{
    public class CommentDto : AuditedEntityDto<Guid>
    {
        public string Text { get; set; }
        public Guid BlogPostId { get; set; }
        public string? CreatorUserName { get; set; }
    }
}
