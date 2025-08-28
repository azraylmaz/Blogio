using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace Blogio.Blog.Workers
{
    /// <summary>
    /// Her gün bir kez çalışır; 5 yıldan eski yazıları ve bağlı verilerini siler.
    /// </summary>
    public class OldPostCleanupWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private readonly IRepository<BlogPost, Guid> _postRepo;
        private readonly IRepository<Comment, Guid> _commentRepo;
        private readonly IRepository<BlogPostTag> _postTagRepo;
        private readonly IRepository<BlogPostLike, Guid> _likeRepo;
        private readonly IClock _clock;
        private readonly IUnitOfWorkManager _uowManager;

        public OldPostCleanupWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            IRepository<BlogPost, Guid> postRepo,
            IRepository<Comment, Guid> commentRepo,
            IRepository<BlogPostTag> postTagRepo,
            IRepository<BlogPostLike, Guid> likeRepo,
            IClock clock,
            IUnitOfWorkManager uowManager)
            : base(timer, serviceScopeFactory)
        {
            _postRepo = postRepo;
            _commentRepo = commentRepo;
            _postTagRepo = postTagRepo;
            _likeRepo = likeRepo;
            _clock = clock;
            _uowManager = uowManager;

            // Günlük çalışsın (test için daha kısa yapabilirsin)
            Timer.Period = (int)TimeSpan.FromDays(1).TotalMilliseconds;
            // İstersen ilk çalışmayı geciktirmek için:
            // Timer.RunOnStart = false; // ilk tick'i beklesin
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext context)
        {
            // Tek bir UoW içinde toplu çalış
            using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

            var cutoff = _clock.Now.AddYears(-5); // 5 yıldan eski

            // Silinecek yazı id’lerini çek
            var ids = (await _postRepo.GetQueryableAsync())
                      .Where(p => p.CreationTime < cutoff)
                      .Select(p => p.Id)
                      .ToList();

            if (ids.Count > 0)
            {
                // Bağlı verileri temizle (FK hatası yememek için)
                await _commentRepo.DeleteAsync(x => ids.Contains(x.BlogPostId));
                await _postTagRepo.DeleteAsync(x => ids.Contains(x.BlogPostId));
                await _likeRepo.DeleteAsync(x => ids.Contains(x.BlogPostId));

                // Yazıların kendisi
                await _postRepo.DeleteAsync(x => ids.Contains(x.Id));
            }

            await uow.CompleteAsync();
        }
    }
}
