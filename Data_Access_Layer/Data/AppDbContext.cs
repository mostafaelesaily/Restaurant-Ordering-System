using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Domain_Layer.Entities;
using Microsoft.EntityFrameworkCore;
namespace Data_Access_Layer.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<MenuItems> MenuItems { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<OrderCoupon> OrderCoupons { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Reservations> Reservations { get; set; }
        public DbSet<Reviews> Reviews { get; set; }
        public DbSet<Tables> Tables { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Configuration Start :

            // Cart Configuration
            builder.Entity<Cart>().ToTable("Cart");
            builder.Entity<Cart>().HasKey(c => c.Id);
            builder.Entity<Cart>().HasIndex(c => c.CustomerId);

            // CartItem Configuration (composite key)
            builder.Entity<CartItem>().ToTable("CartItem");
            builder.Entity<CartItem>().HasKey(ci => ci.Id);

            // Categories Configuration
            builder.Entity<Categories>().ToTable("Category");
            builder.Entity<Categories>().HasKey(cat => cat.id);
            builder.Entity<Categories>().Property(n => n.name)
                .HasMaxLength(100).IsRequired();
            builder.Entity<Categories>().Property(d => d.description)
                .HasMaxLength(500);
            builder.Entity<Categories>().Property(i => i.ImageUrl)
                .HasMaxLength(2048);
            builder.Entity<Categories>().HasIndex(cat => cat.name);

            // MenuItem Configuration
            builder.Entity<MenuItems>().ToTable("MenuItem");
            builder.Entity<MenuItems>().HasKey(mi => mi.id);
            builder.Entity<MenuItems>().HasIndex(mi => mi.name);
            builder.Entity<MenuItems>().HasIndex(mi => mi.categoryId);
            builder.Entity<MenuItems>().Property(mi => mi.name).IsRequired().HasMaxLength(150);
            builder.Entity<MenuItems>().Property(mi => mi.description).HasMaxLength(1000);
            builder.Entity<MenuItems>().Property(mi => mi.price).HasPrecision(18, 2);
            builder.Entity<MenuItems>().Property(mi => mi.ImageUrl).HasMaxLength(2048);

            // Coupon Configuration
            builder.Entity<Coupon>().ToTable("Coupon");
            builder.Entity<Coupon>().HasKey(c => c.Id);
            builder.Entity<Coupon>().Property(c => c.Code).IsRequired().HasMaxLength(50);
            builder.Entity<Coupon>().Property(d => d.Discount).HasPrecision(18, 2);
            builder.Entity<Coupon>().Property(c => c.ExpireDate).IsRequired();
            builder.Entity<Coupon>().Property(c => c.MaxUsage).IsRequired();
            builder.Entity<Coupon>().Property(c => c.CurrentUsage).IsRequired();
            builder.Entity<Coupon>().HasIndex(c => c.Code).IsUnique(true);

            // Favorite Configuration (composite key)
            builder.Entity<Favorite>().ToTable("Favorite");
            builder.Entity<Favorite>().HasKey(f => f.Id);

            // Notification Configuration
            builder.Entity<Notifications>().ToTable("Notification");
            builder.Entity<Notifications>().HasKey(n => n.Id);
            builder.Entity<Notifications>().HasIndex(n => n.UserId);
            builder.Entity<Notifications>().HasIndex(n => n.Title);
            builder.Entity<Notifications>().Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Entity<Notifications>().Property(n => n.Message).HasMaxLength(2000);

            // OrderCoupon Configuration (composite key)
            builder.Entity<OrderCoupon>().ToTable("OrderCoupon");
            builder.Entity<OrderCoupon>().HasKey(oc => oc.id);

            // OrderItem Configuration
            builder.Entity<OrderItems>().ToTable("OrderItem");
            builder.Entity<OrderItems>().HasKey(oi => oi.id);
            builder.Entity<OrderItems>().HasIndex(oi => oi.orderId);
            builder.Entity<OrderItems>().Property(oi => oi.unitPrice).HasPrecision(18, 2);

            // Order Configuration
            builder.Entity<Orders>().ToTable("Order");
            builder.Entity<Orders>().HasKey(o => o.id);
            builder.Entity<Orders>().HasIndex(o => o.customerId);
            builder.Entity<Orders>().HasIndex(o => o.CreatedAt);
            builder.Entity<Orders>().HasIndex(o => o.Status);
            builder.Entity<Orders>().Property(o => o.Address).HasMaxLength(500);
            builder.Entity<Orders>().Property(o => o.Notes).HasMaxLength(1000);
            builder.Entity<Orders>().Property(o => o.TotalPrice).HasPrecision(18, 2);

            // Reservation Configuration
            builder.Entity<Reservations>().ToTable("Reservation");
            builder.Entity<Reservations>().HasKey(r => r.Id);
            builder.Entity<Reservations>().HasIndex(r => r.custoemerId);
            builder.Entity<Reservations>().HasIndex(r => r.tableId);
            builder.Entity<Reservations>().HasIndex(r => r.ReservationDate);
            builder.Entity<Reservations>().Property(n => n.NumberOfGuests).
                IsRequired();

            // Review Configuration
            builder.Entity<Reviews>().ToTable("Review");
            builder.Entity<Reviews>().HasKey(r => r.Id);
            builder.Entity<Reviews>().HasIndex(r => r.MenuItemId);
            builder.Entity<Reviews>().HasIndex(r => r.CustomerId);
            builder.Entity<Reviews>().Property(r => r.Comment).HasMaxLength(2000);
            builder.Entity<Reviews>().Property(r => r.Rating).IsRequired();

            // Table Configuration
            builder.Entity<Tables>().ToTable("Table");
            builder.Entity<Tables>().HasKey(t => t.Id);
            builder.Entity<Tables>().HasIndex(t => t.TableNumber).IsUnique();
            builder.Entity<Tables>().Property(t => t.QrCode).HasMaxLength(500);
            builder.Entity<Tables>().Property(t => t.Capacity).IsRequired();
            builder.Entity<Tables>().Property(t => t.TableNumber);
            // Configuration End.

            // Relations Start :
     
            // Cart - User (one-to-one)
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.cart)
                .HasForeignKey<Cart>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // CartItem relations
            builder.Entity<CartItem>()
                .HasOne(ci => ci.cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.menuItems)
                .WithMany(mi => mi.cartItems)
                .HasForeignKey(ci => ci.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // MenuItem - Category
            builder.Entity<MenuItems>()
                .HasOne(mi => mi.categories)
                .WithMany(c => c.items)
                .HasForeignKey(mi => mi.categoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Favorite relations
            builder.Entity<Favorite>()
                .HasOne(f => f.MenuItems)
                .WithMany(mi => mi.favorites)
                .HasForeignKey(f => f.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification - User
            builder.Entity<Notifications>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderCoupon relations
            builder.Entity<OrderCoupon>()
                .HasOne(oc => oc.order)
                .WithMany(o => o.orderCoupons)
                .HasForeignKey(oc => oc.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderCoupon>()
                .HasOne(oc => oc.coupon)
                .WithMany(c => c.orderCoupons)
                .HasForeignKey(oc => oc.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItems relations
            builder.Entity<OrderItems>()
                .HasOne(oi => oi.order)
                .WithMany(o => o.orderItems)
                .HasForeignKey(oi => oi.orderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItems>()
                .HasOne(oi => oi.menuItems)
                .WithMany(mi => mi.orderItems)
                .HasForeignKey(oi => oi.menuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Orders - Users (customer, cheif, delivery)
            builder.Entity<Orders>()
                 .HasOne(o => o.AppUser)
                 .WithMany(u => u.Userorders)
                 .HasForeignKey(o => o.customerId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Orders>()
                .HasOne(o => o.Cheif)
                .WithMany(u => u.Cheiforders)
                .HasForeignKey(o => o.CheifId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Orders>()
               .HasOne(o => o.DeliveryUser)
               .WithMany(u => u.Deliveryorders)
               .HasForeignKey(o => o.DeliveryId)
               .OnDelete(DeleteBehavior.Restrict);

            // Orders - Table (shadow FK TableId)
            builder.Entity<Orders>()
                .HasOne(o => o.Tables)
                .WithMany(t => t.orders)
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservations relations
            builder.Entity<Reservations>()
                .HasOne(r => r.Tables)
                .WithMany(t => t.reservations)
                .HasForeignKey(r => r.tableId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservations>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.custoemerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reviews relations
            builder.Entity<Reviews>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reviews>()
                .HasOne(r => r.MenuItems)
                .WithMany(mi => mi.reviews)
                .HasForeignKey(r => r.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            // Relations End .

        }

    }
}
