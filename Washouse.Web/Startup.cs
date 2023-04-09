using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Washouse.Common.Mails;
using Washouse.Common.Utils;
using Washouse.Data;
using Washouse.Data.Infrastructure;
using Washouse.Data.Repositories;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Washouse.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Washouse.Web", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.Configure<AppSetting>(Configuration.GetSection("AppSettings"));
            services.Configure<GCSConfigOptions>(Configuration.GetSection("GCSConfigOptions"));
            //services.Configure<GCSConfigOptions>(Configuration.GetSection("GoogleCloudStorageBucketName"));
            services.AddDbContext<WashouseDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("WashouseDB")));

            var secretKey = Configuration["AppSettings:SecretKey"];
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            services.AddAuthentication(options =>
            {
                /*options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;*/
                //fix 06/04 by DatLT, comment and add new code
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

            })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

                        ClockSkew = TimeSpan.Zero
                    };
                    /*options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidAudience = Configuration["JWT:ValidAudience"],
                        ValidIssuer = Configuration["JWT:ValidIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes)
                    };*/
                })
                .AddGoogle(GoogleDefaults.AuthenticationScheme, googleOptions =>
                {
                    // Đọc thông tin Authentication:Google từ appsettings.json
                    IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");

                    // Thiết lập ClientID và ClientSecret để truy cập API google
                    googleOptions.ClientId = googleAuthNSection["ClientId"];
                    googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
                    // Cấu hình Url callback lại từ Google (không thiết lập thì mặc định là /signin-google)
                    //googleOptions.CallbackPath = "/loginWithGoogle";
                    googleOptions.SaveTokens = true;
                    googleOptions.Scope.Add("https://www.googleapis.com/auth/user.phonenumbers.read");


                })
                .AddCookie(options =>
                {
                    options.Cookie.Name = "YourCookieNameHere";
                    options.LoginPath = "/api/logins/login/google";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    //fix 06/04 by DatLT, add new code
                    /*options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = context =>
                        {
                            if (!context.Principal.Identity.IsAuthenticated &&
                                !context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                            {
                                // Return 401 status code for non-API requests when the user is not authenticated
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            }
                            return Task.CompletedTask;
                        }
                    };*/
                });

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
            var mailsettings = Configuration.GetSection("MailSettings");
            services.Configure<MailSettings>(mailsettings);
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
            var vnpaysettings = Configuration.GetSection("VNPaySettings");
            services.Configure<VNPaySettings>(vnpaysettings);
            services.AddTransient<IPaymentRepository, PaymentRepository>();
            services.AddTransient<IPaymentService, PaymentService>();
            services.AddTransient<IDeliveryRepository, DeliveryRepository>();
            var twiliosettings = Configuration.GetSection("Twilio");
            services.Configure<TwilioSettings>(twiliosettings);
            services.AddTransient<ISMSService, SMSService>();
            services.AddTransient<IWalletRepository, WalletRepository>();
            services.AddTransient<IWalletService, WalletService>();
            services.AddTransient<IWalletTransactionRepository, WalletTransactionRepository>();
            services.AddTransient<IWalletTransactionService, WalletTransactionService>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<INotificationAccountRepository, NotificationAccountRepository>();
            services.AddTransient<INotificationAccountService, NotificationAccountService>();
            services.AddStackExchangeRedisCache(options => { options.Configuration = Configuration["RedisCacheUrl"]; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Washouse.Web v1"));
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Washouse.Web v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
