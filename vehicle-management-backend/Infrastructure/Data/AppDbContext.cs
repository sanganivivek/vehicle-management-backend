using Microsoft.EntityFrameworkCore;
using vehicle_management_backend.Core.Models;
namespace vehicle_management_backend.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<VehicleMaster> Vehicles { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VehicleMaster>()
                .HasKey(v => v.VehicleId);

            modelBuilder.Entity<VehicleMaster>()
                 .HasOne(v => v.Brand)
                  .WithMany()
                 .HasForeignKey(v => v.BrandId)
                 .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            modelBuilder.Entity<VehicleMaster>()
                .HasOne(v => v.Model)
                .WithMany()
                .HasForeignKey(v => v.ModelId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            // Booking Relationships
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany()
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Dealer)
                .WithMany()
                .HasForeignKey(b => b.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // BookingNumber auto-increments on each insert (sequential display ID)
            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingNumber)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<VehicleMaster>()
                .Property(p => p.OneDayRate)
                .HasColumnType("decimal(18,2)"); // 18 digits total, 2 decimal places
        }
    }
}