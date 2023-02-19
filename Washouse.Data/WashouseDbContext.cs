﻿using Microsoft.EntityFrameworkCore;
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
    public class WashouseDbContext : DbContext
    {
        public WashouseDbContext()
        {
        }

        public WashouseDbContext(DbContextOptions<WashouseDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Center> Centers { get; set; }
        public virtual DbSet<Delivery> Deliveries { get; set; }
        public virtual DbSet<District> Districts { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<ServicePrice> ServicePrices { get; set; }
        public virtual DbSet<Tracking> Trackings { get; set; }
        public virtual DbSet<Ward> Wards { get; set; }
        public virtual DbSet<Error> Errors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetail>()
                .HasKey(m => new { m.OrderId, m.ServiceId });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetParent(Directory.GetCurrentDirectory()).ToString() + "\\Washouse.Web")
                   .AddJsonFile("appsettings.json")
                   .Build();
                var connectionString = configuration.GetConnectionString("WashouseDB");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

    }
}
