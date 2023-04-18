using Washouse.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Washouse.Data;
using Washouse.Data.Repositories;
using Washouse.Service.Interface;
using Washouse.Service.Implement;
using Microsoft.AspNetCore.Http;
using Washouse.Common.Mails;
using Washouse.Data.Infrastructure;
using Washouse.Model.RequestModels;

namespace HostelManagementWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    services.AddDbContext<WashouseDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("WashouseDB")));
                    services.AddSingleton<INotificationRepository, NotificationRepository>();
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<ICloudStorageService, CloudStorageService>();
                    services.AddTransient<IUnitOfWork, UnitOfWork>();
                    services.AddTransient<IDbFactory, DbFactory>();
                    services.AddTransient<ICenterService, CenterService>();
                    services.AddTransient<ICenterRepository, CenterRepository>();
                    services.AddTransient<IServiceCategoryService, ServiceCategoryService>();
                    services.AddTransient<IServiceCategoryRepository, ServiceCategoryRepository>();
                    services.AddTransient<IServiceService, ServiceService>();
                    services.AddTransient<IServiceRepository, ServiceRepository>();
                    services.AddTransient<IAccountService, AccountService>();
                    services.AddTransient<IAccountRepository, AccountRepository>();
                    services.AddTransient<IDistrictService, DistrictService>();
                    services.AddTransient<IDistrictRepository, DistrictRepository>();
                    services.AddTransient<IWardRepository, WardRepository>();
                    services.AddTransient<IWardService, WardService>();
                    services.AddTransient<ILocationRepository, LocationRepository>();
                    services.AddTransient<ILocationService, LocationService>();
                    services.AddTransient<ICustomerService, CustomerService>();
                    services.AddTransient<ICustomerRepository, CustomerRepository>();
                    services.AddTransient<IStaffService, StaffService>();
                    services.AddTransient<IStaffReposity, StaffRepository>();
                    services.AddTransient<IOrderTrackingRepository, OrderTrackingRepository>();
                    services.AddTransient<IOrderDetailTrackingRepository, OrderDetailTrackingRepository>();
                    services.AddTransient<IOrderDetailTrackingService, OrderDetailTrackingService>();
                    services.AddTransient<IOperatingHourRepository, OperatingHourRepository>();
                    services.AddTransient<IOperatingHourService, OperatingHourService>();

                    //services.AddTransient<ISettingsService, SettingsService>();
                    services.AddTransient<ICustomerRepository, CustomerRepository>();
                    services.AddTransient<ICustomerService, CustomerService>();
                    services.AddTransient<IStaffReposity, StaffRepository>();
                    services.AddTransient<IStaffService, StaffService>();
                    services.AddOptions();
                    services.AddTransient<ISendMailService, SendMailService>();
                    services.AddTransient<IPostRepository, PostRepository>();
                    services.AddTransient<IPostService, PostService>();
                    services.AddTransient<IPromotionRepository, PromotionRepository>();
                    services.AddTransient<IPromotionService, PromotionService>();
                    services.AddTransient<IFeedbackRepository, FeedbackRepository>();
                    services.AddTransient<IFeedbackService, FeedbackService>();
                    services.AddTransient<IDeliveryPriceChartRepository, DeliveryPriceChartRepository>();
                    services.AddTransient<IOrderDetailRepository, OrderDetailRepository>();
                    services.AddTransient<IOrderRepository, OrderRepository>();
                    services.AddTransient<IOrderService, OrderService>();
                    services.AddTransient<IServicePriceRepository, ServicePriceRepository>();
                    services.AddTransient<IServiceGalleryRepository, ServiceGalleryRepository>();
                    services.AddTransient<INotificationRepository, NotificationRepository>();
                    services.AddTransient<INotificationService, NotificationService>();
                    services.AddTransient<ICenterRequestRepository, CenterRequestRepository>();
                    services.AddTransient<ICenterRequestService, CenterRequestService>();
                    services.AddOptions();
                    services.AddTransient<IPaymentRepository, PaymentRepository>();
                    services.AddTransient<IPaymentService, PaymentService>();
                    services.AddTransient<IDeliveryRepository, DeliveryRepository>();
                    services.AddTransient<IDeliveryService, DeliveryService>();
                    services.AddTransient<ISMSService, SMSService>();
                    services.AddTransient<IWalletRepository, WalletRepository>();
                    services.AddTransient<IWalletService, WalletService>();
                    services.AddTransient<IWalletTransactionRepository, WalletTransactionRepository>();
                    services.AddTransient<IWalletTransactionService, WalletTransactionService>();
                    services.AddTransient<ITransactionRepository, TransactionRepository>();
                    services.AddTransient<ITransactionService, TransactionService>();
                    services.AddTransient<INotificationAccountRepository, NotificationAccountRepository>();
                    services.AddTransient<INotificationAccountService, NotificationAccountService>();
                    services.AddOptions();
                    services.AddHostedService<Worker>();
                });
    }
}
