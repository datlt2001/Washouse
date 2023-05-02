using System.Security.Claims;
using Washouse.Common.Mails;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Interface;

namespace Washouse.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPostService _postService;
        private readonly IOrderService _orderService;
        private readonly ISendMailService _sendMailService;

        public Worker(ILogger<Worker> logger, IPostService postService, IOrderService orderService, ISendMailService sendMailService)
        {
            _logger = logger;
            _postService = postService;
            _orderService = orderService;
            _sendMailService = sendMailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //UC131An email will be sent to the manager of the center to remind him/her to confirm the order pending 3h ago.
                var pendingOrders = await _orderService.GetPendingOrders3HoursAgo();
                foreach (var item in pendingOrders.ToList())
                {
                    var account = item.OrderDetails.FirstOrDefault().Service.Center.staff.FirstOrDefault(staff => staff.IsManager == true).Account;
                    _logger.LogInformation("{account}", account.Email);
                    var sendEmail = account.Email;
                    string path = "./Templates_email/CreateOrder.txt";
                    string content = System.IO.File.ReadAllText(path);
                    content = content.Replace("{recipient}",
                        account.FullName);
                    content = content.Replace("{orderId}", item.Id);
                    content = content.Replace("{phone}", account.Phone);
                    await _sendMailService.SendEmailAsync(sendEmail, "Tạo đơn hàng", content);

                    _logger.LogInformation("Order {id} has been remind to {email}", item.Id, sendEmail);
                }
                //UC132
                //UC133:Publish a scheduled post
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