using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Data
{
    public partial class WashouseDbContext : DbContext
    {
        public WashouseDbContext()
        {
        }

        public WashouseDbContext(DbContextOptions<WashouseDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AdditionService> AdditionServices { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Center> Centers { get; set; }
        public virtual DbSet<CenterGallery> CenterGalleries { get; set; }
        public virtual DbSet<CenterRequest> CenterRequests { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<DaysOfWeek> DaysOfWeeks { get; set; }
        public virtual DbSet<Delivery> Deliveries { get; set; }
        public virtual DbSet<DeliveryPriceChart> DeliveryPriceCharts { get; set; }
        public virtual DbSet<District> Districts { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<NotificationAccount> NotificationAccounts { get; set; }
        public virtual DbSet<OperatingHour> OperatingHours { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<OrderDetailTracking> OrderDetailTrackings { get; set; }
        public virtual DbSet<OrderTracking> OrderTrackings { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Promotion> Promotions { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<Resourse> Resourses { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<ServiceGallery> ServiceGalleries { get; set; }
        public virtual DbSet<ServicePrice> ServicePrices { get; set; }
        //public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }
        public virtual DbSet<Staff> Staffs { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<Wallet> Wallets { get; set; }
        public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }
        public virtual DbSet<Ward> Wards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.Phone, "IX_Accounts")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "IX_Accounts_Email")
                    .IsUnique();

                entity.HasIndex(e => e.LocationId, "IX_Accounts_LocationId");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Dob).HasColumnType("date");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastLogin).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.ProfilePic)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_Accounts_Locations");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.WalletId)
                    .HasConstraintName("FK_Accounts_Wallets");
            });

            modelBuilder.Entity<AdditionService>(entity =>
            {
                entity.HasIndex(e => e.CenterId, "IX_AdditionServices_CenterId");

                entity.Property(e => e.AdditionName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Alias).HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Image)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.AdditionServices)
                    .HasForeignKey(d => d.CenterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AdditionService_Centers");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Alias).HasMaxLength(50);

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Image)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Center>(entity =>
            {
                entity.HasIndex(e => e.TaxCode, "IX_Centers")
                    .IsUnique();

                entity.HasIndex(e => e.LocationId, "IX_Centers_LocationId");

                entity.Property(e => e.Alias).HasMaxLength(50);

                entity.Property(e => e.CenterName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Image)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.MonthOff)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.Rating).HasColumnType("decimal(2, 1)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TaxCode)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.TaxRegistrationImage)
                    //.IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.LastDeactivate).HasColumnType("datetime");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Centers)
                    .HasForeignKey(d => d.LocationId);

                entity.HasOne(d => d.Wallet)
                   .WithMany(p => p.Centers)
                   .HasForeignKey(d => d.WalletId)
                   .HasConstraintName("FK_Centers_Wallets");
            });

            modelBuilder.Entity<CenterGallery>(entity =>
            {
                entity.HasIndex(e => e.CenterId, "IX_CenterGalleries_CenterId");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Image)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.CenterGalleries)
                    .HasForeignKey(d => d.CenterId);
            });

            modelBuilder.Entity<CenterRequest>(entity =>
            {
                entity.HasIndex(e => e.CenterRequesting, "IX_CenterRequests_CenterRequesting");

                entity.Property(e => e.Alias).HasMaxLength(50);

                entity.Property(e => e.CenterName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Image)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.MonthOff)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.Rating).HasColumnType("decimal(2, 1)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TaxCode)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.TaxRegistrationImage)
                    //.IsRequired()
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                
                entity.Property(e => e.LastDeactivate).HasColumnType("datetime");

                entity.HasOne(d => d.CenterRequestingNavigation)
                    .WithMany(p => p.CenterRequests)
                    .HasForeignKey(d => d.CenterRequesting)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_Customers_AccountId");

                entity.HasIndex(e => e.Address, "IX_Customers_Address");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Fullname)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.AddressNavigation)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.Address)
                    .HasConstraintName("FK_Customers_Locations");
            });

            modelBuilder.Entity<DaysOfWeek>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DayName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Delivery>(entity =>
            {
                entity.HasIndex(e => e.LocationId, "IX_Deliveries_LocationId");

                entity.HasIndex(e => e.OrderId, "IX_Deliveries_OrderId");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ShipperName)
                    .HasMaxLength(50);

                entity.Property(e => e.ShipperPhone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Deliveries)
                    .HasForeignKey(d => d.LocationId);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Deliveries)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasForeignKey(d => d.OrderId);
            });

            modelBuilder.Entity<DeliveryPriceChart>(entity =>
            {
                entity.HasIndex(e => e.CenterId, "IX_DeliveryPriceCharts_CenterId");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.MaxDistance).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.MaxWeight).HasColumnType("decimal(6, 3)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.DeliveryPriceCharts)
                    .HasForeignKey(d => d.CenterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DeliveryPriceCharts_Centers");
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.Property(e => e.DistrictName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasIndex(e => e.CenterId, "IX_Feedbacks_CenterId");

                entity.HasIndex(e => e.ServiceId, "IX_Feedbacks_ServiceId");

                entity.HasIndex(e => e.OrderId, "IX_Feedbacks_OrderId");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ReplyBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReplyDate).HasColumnType("datetime");

                entity.Property(e => e.ReplyMessage).HasMaxLength(500);

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.CenterId)
                    .HasConstraintName("FK_Feedbacks_Centers");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_Feedbacks_Orders");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("FK_Feedbacks_Services");

            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasIndex(e => e.WardId, "IX_Locations_WardId");

                entity.Property(e => e.AddressString)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Latitude).HasColumnType("decimal(12, 9)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(12, 9)");

                entity.HasOne(d => d.Ward)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.WardId);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(e => e.OrderId, "IX_Notifications_OrderId");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Notifications_Orders");
            });

            modelBuilder.Entity<NotificationAccount>(entity =>
            {
                entity.HasKey(e => new { e.NotificationId, e.AccountId });

                entity.HasIndex(e => e.AccountId, "IX_NotificationAccounts_AccountId");

                entity.Property(e => e.ReadDate).HasColumnType("datetime");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.NotificationAccounts)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NotificationAccounts_Accounts");

                entity.HasOne(d => d.Notification)
                    .WithMany(p => p.NotificationAccounts)
                    .HasForeignKey(d => d.NotificationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NotificationAccounts_Notifications");
            });

            modelBuilder.Entity<OperatingHour>(entity =>
            {
                entity.HasKey(e => new { e.CenterId, e.DaysOfWeekId });

                entity.HasIndex(e => e.DaysOfWeekId, "IX_OperatingHours_DaysOfWeekId");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.OperatingHours)
                    .HasForeignKey(d => d.CenterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatingHours_Centers");

                entity.HasOne(d => d.DaysOfWeek)
                    .WithMany(p => p.OperatingHours)
                    .HasForeignKey(d => d.DaysOfWeekId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OperatingHours_DaysOfWeeks");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.CustomerId, "IX_Orders_CustomerId");

                entity.HasIndex(e => e.LocationId, "IX_Orders_LocationId");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CustomerEmail)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerMessage).HasMaxLength(500);

                entity.Property(e => e.CancelReasonByStaff).HasMaxLength(100);

                entity.Property(e => e.CancelReasonByCustomer).HasMaxLength(100);

                entity.Property(e => e.CustomerMobile)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DeliveryType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.PreferredDeliverTime).HasColumnType("datetime");

                entity.Property(e => e.PreferredDropoffTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId);

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.LocationId);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasIndex(e => e.OrderId, "IX_OrderDetails_OrderId");

                entity.HasIndex(e => e.ServiceId, "IX_OrderDetails_ServiceId");

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Measurement).HasColumnType("decimal(8, 3)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetails)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasForeignKey(d => d.OrderId);

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.OrderDetails)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasForeignKey(d => d.ServiceId);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasIndex(e => e.OrderId, "IX_Payments_OrderId");

                entity.HasIndex(e => e.PromoCode, "IX_Payments_PromoCode");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Discount).HasColumnType("decimal(5, 4)");

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PlatformFee).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Payments_Orders");

                entity.HasOne(d => d.PromoCodeNavigation)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.PromoCode)
                    .HasConstraintName("FK_Payments_Promotions1");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(e => e.AuthorId, "IX_Posts_AuthorId");

                entity.Property(e => e.Content).HasColumnType("nvarchar(max)"); 

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Thumbnail)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.AuthorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Posts_Accounts");
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasIndex(e => e.CenterId, "IX_Promotions_CenterId");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Discount).HasColumnType("decimal(5, 4)");

                entity.Property(e => e.ExpireDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.Promotions)
                    .HasForeignKey(d => d.CenterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Promotions_Centers");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_RefreshTokens_AccountId");

                entity.Property(e => e.ExpiredAt).HasColumnType("datetime");

                entity.Property(e => e.IssuedAt).HasColumnType("datetime");

                entity.Property(e => e.JwtId)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Token)
                    .IsRequired()
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RefreshTokens_Accounts");
            });

            modelBuilder.Entity<Resourse>(entity =>
            {
                entity.HasIndex(e => e.CenterId, "IX_Resourses_CenterId");

                entity.Property(e => e.Alias).HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DryCapacity).HasColumnType("decimal(7, 3)");

                entity.Property(e => e.ResourceName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.WashCapacity).HasColumnType("decimal(7, 3)");

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.Resourses)
                    .HasForeignKey(d => d.CenterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Resourses_Centers");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasIndex(e => e.CategoryId, "IX_Services_CategoryId");

                entity.HasIndex(e => e.CenterId, "IX_Services_CenterId");

                entity.Property(e => e.Alias)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Image).HasMaxLength(256);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MinPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Rating).HasColumnType("decimal(2, 1)");

                entity.Property(e => e.ServiceName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.CategoryId);

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.CenterId);
            });

            modelBuilder.Entity<ServiceGallery>(entity =>
            {
                entity.HasIndex(e => e.ServiceId, "IX_ServiceGalleries_ServiceId");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Image)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ServiceGalleries)
                    .HasForeignKey(d => d.ServiceId);
            });

            modelBuilder.Entity<ServicePrice>(entity =>
            {
                entity.HasIndex(e => e.ServiceId, "IX_ServicePrices_ServiceId");

                entity.Property(e => e.MaxValue).HasColumnType("decimal(8, 3)");

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ServicePrices)
                    .HasForeignKey(d => d.ServiceId);
            });

            /*modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasIndex(e => e.ServiceRequesting, "IX_ServiceRequests_ServiceRequesting");

                entity.Property(e => e.Alias)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Image).HasMaxLength(256);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MinPrice).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 3)");

                entity.Property(e => e.Rating).HasColumnType("decimal(2, 1)");

                entity.Property(e => e.ServiceName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.ServiceRequestingNavigation)
                    .WithMany(p => p.ServiceRequests)
                    .HasForeignKey(d => d.ServiceRequesting)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });
*/
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_Staffs_AccountId");

                entity.HasIndex(e => e.CenterId, "IX_Staffs_CenterId");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.IdBackImg)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.IdFrontImg)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.IdNumber)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.staff)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Center)
                    .WithMany(p => p.staff)
                    .HasForeignKey(d => d.CenterId);
            });

            modelBuilder.Entity<OrderTracking>(entity =>
            {
                entity.HasIndex(e => e.OrderId, "IX_OrderTrackings_OrderId");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderTrackings)
                    .HasForeignKey(d => d.OrderId);
            });

            modelBuilder.Entity<OrderDetailTracking>(entity =>
            {
                entity.HasIndex(e => e.OrderDetailId, "IX_OrderDetailTrackings_OrderId");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.OrderDetailId)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.HasOne(d => d.OrderDetail)
                    .WithMany(p => p.OrderDetailTrackings)
                    .HasForeignKey(d => d.OrderDetailId);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.WalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Transactions_Wallets");
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateTimeStamp).HasColumnType("datetime");

                entity.HasOne(d => d.FromWallet)
                    .WithMany(p => p.WalletTransactionFromWallets)
                    .HasForeignKey(d => d.FromWalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletTransactions_Wallets_From");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.WalletTransactions)
                    .HasForeignKey(d => d.PaymentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletTransactions_Payments");

                entity.HasOne(d => d.ToWallet)
                    .WithMany(p => p.WalletTransactionToWallets)
                    .HasForeignKey(d => d.ToWalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_WalletTransactions_Wallets_To");
            });

            modelBuilder.Entity<Ward>(entity =>
            {
                entity.HasIndex(e => e.DistrictId, "IX_Wards_DistrictId");

                entity.Property(e => e.WardName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Wards)
                    .HasForeignKey(d => d.DistrictId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration;
                /*if (Directory.Exists(Directory.GetCurrentDirectory() + "\\appsettings.json"))
                {
                    configuration = new ConfigurationBuilder()
                                   .SetBasePath(Directory.GetCurrentDirectory())
                                   .AddJsonFile("appsettings.json")
                                   .Build();
                }
                else
                {
                    configuration = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).ToString() + "\\Washouse.Web")
                       .AddJsonFile("appsettings.json")
                       .Build();
                }*/
                /*configuration = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).ToString() + "\\Washouse.Web")
                       .AddJsonFile("appsettings.json")
                       .Build();*/
                configuration = new ConfigurationBuilder()
                                   .SetBasePath(Directory.GetCurrentDirectory())
                                   .AddJsonFile("appsettings.json")
                                   .Build();
                var connectionString = configuration.GetConnectionString("WashouseDB");
                //var connectionString = "Server=washouse.database.windows.net;Uid=washouseAdmin;Pwd=Washouse123!;Database= WashouseDb ";
                optionsBuilder.UseSqlServer(connectionString);
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

    }
}
