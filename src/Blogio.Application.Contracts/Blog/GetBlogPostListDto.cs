using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Blogio.Blog
{
    // Listeleme/arama için filtre DTO’su
    public class GetBlogPostListDto : PagedAndSortedResultRequestDto
    {
        // Title/Content üzerinde arama
        public string? Filter { get; set; }

        public bool? IsPublished { get; set; }
        public Guid? AuthorId { get; set; }
        public List<Guid>? TagIds { get; set; }

        // Tarih filtresi (CreationTime için)
        public DateTime? CreationTimeStart { get; set; }
        public DateTime? CreationTimeEnd { get; set; }
    }
}
