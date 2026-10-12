using AIBusinessSupport.Api.Models;
using AIBusinessSupport.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AIBusinessSupport.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

        public DbSet<Document> Documents => Set<Document>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(x => x.Name)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Phone)
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(x => x.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Price)
                    .HasPrecision(18, 2);

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(x => x.Status)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.TotalAmount)
                    .HasPrecision(18, 2);

                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.Orders)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(x => x.UnitPrice)
                    .HasPrecision(18, 2);

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                    .WithMany(x => x.OrderItems)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.Property(x => x.Subject)
                    .HasMaxLength(250)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Priority)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.SupportTickets)
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.Property(x => x.FileName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.ContentType)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasMaxLength(50)
                    .IsRequired();
            });
        }
    }
}
