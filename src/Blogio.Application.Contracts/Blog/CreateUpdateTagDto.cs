using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blogio.Blog
{
    public class CreateUpdateTagDto
    {
        [Required]
        [StringLength(64)]
        public string Name { get; set; }
    }
}
