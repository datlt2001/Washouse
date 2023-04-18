using Washouse.Service.Interface;

namespace Washouse.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPostService _postService;

        public Worker(ILogger<Worker> logger, IPostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var allPost = _postService.GetAll();
                allPost = allPost.Where(post => post.Status.Trim().ToLower().Equals("scheduled")).ToList();
                foreach (var item in allPost)
                {
                    if (DateTime.Now > item.UpdateDate)
                    {
                        item.Status = "Published";
                        item.UpdateDate = DateTime.Now;
                        await _postService.Update(item);
                        _logger.LogInformation("Post {id} has been published {time}",item.Id, item.UpdateDate);
                    }
                }
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}