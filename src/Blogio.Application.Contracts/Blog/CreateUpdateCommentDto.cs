using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogio.Blog
{
    public class CreateUpdateCommentDto
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public Guid BlogPostId { get; set; }
    }
}
