using System.Collections.Generic;

namespace Blogio.Blazor.Models
{
    public class ListResult<T>
    {
        public List<T> Items { get; set; } = new();
    }
}
