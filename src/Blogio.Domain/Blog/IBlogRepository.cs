using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Blogio.Blog
{
    public interface IBlogRepository : IRepository<BlogPost, Guid>
    {
        Task<List<BlogPost>> GetBlogPostsAsync();
        Task<BlogPost> GetBlogPostByIdAsync(Guid id);
        TaskCreationOptions CreateBlogPostAsync(BlogPost blogPost);
        Task UpdateBlogPostAsync(BlogPost blogPost);
        Task DeleteBlogPostAsync(Guid id);
      

    }
}
