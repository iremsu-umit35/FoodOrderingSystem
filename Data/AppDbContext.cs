using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using FoodOrderingSystem.Models.Entities;
using FoodOrderingSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FoodOrderingSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewReply> ReviewReply { get; set; }
        public DbSet<FoodPriceHistory> FoodPriceHistory { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<OrderTracking> OrderTracking { get; set; }

        public DbSet<UserDetailView> UserDetailView { get; set; } //adimn tarafından görüntülenen kullanıcı detayları için view modeli
        public DbSet<ReviewAdminViewModel> ReviewAdminView { get; set; } // adimn tarafından görüntülenen yorumlar için view modeli

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDetailView>()
                .HasNoKey()
                .ToView("vw_UserDetails"); //view için admin için

            modelBuilder.Entity<ReviewAdminViewModel>()
            .HasNoKey()
                .ToView("vw_ReviewList"); //view için

            // AverageRating için precision ayarı
            modelBuilder.Entity<Restaurant>()
                 .Property(r => r.AverageRating)
                 .HasPrecision(3, 2); // örn: 4.50 gibi değerler için

            base.OnModelCreating(modelBuilder);


        }
    }
}
