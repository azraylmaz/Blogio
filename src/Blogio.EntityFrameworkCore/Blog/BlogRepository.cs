using Blogio.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Blogio.Blog
{
    public class BlogRepository : EfCoreRepository<BlogioDbContext, BlogPost, Guid>, IBlogRepository
    {
        public BlogRepository(IDbContextProvider<BlogioDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public TaskCreationOptions CreateBlogPostAsync(BlogPost blogPost)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBlogPostAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<BlogPost> GetBlogPostByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<BlogPost>> GetBlogPostsAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateBlogPostAsync(BlogPost blogPost)
        {
            throw new NotImplementedException();
        }
    }
}
