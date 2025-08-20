using System;
using Volo.Abp.Application.Dtos;

namespace Blogio.Blazor.Components.Pages.Blog
{
    public class AuthorDto : EntityDto<Guid>
    {
        public string UserName { get; set; } = "";
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public int PostCount { get; set; }
    }
}
