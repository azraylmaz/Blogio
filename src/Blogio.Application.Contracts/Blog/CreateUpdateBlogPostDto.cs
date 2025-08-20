using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogio.Blog
{
    public class CreateUpdateBlogPostDto
    {
        [Required, StringLength(256)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Guid AuthorId { get; set; }

        // n-n ilişkiyi yönetmek için
        public List<Guid> TagIds { get; set; } = new();

        // İlk oluştururken yayınlamak istemiyorsan göndermeyebilirsin;
        // PublishAsync ile de değiştirilebilir.
        public bool? IsPublished { get; set; }
    }
}
