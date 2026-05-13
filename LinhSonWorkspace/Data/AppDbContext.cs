using Microsoft.EntityFrameworkCore;
using LinhSonWorkspace.Models;

namespace LinhSonWorkspace.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<WorkspaceType> WorkspaceTypes { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Using SQL Server Authentication
            optionsBuilder.UseSqlServer(
                @"Server=TIEN_DUC;Database=LinhSonWorkspaceDB;User Id=ducvps;Password=Mtdvpscom1@;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Booking -> User (CreatedBy) relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.CreatedByUser)
                .WithMany(u => u.CreatedBookings)
                .HasForeignKey(b => b.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Booking -> Customer relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Booking -> Workspace relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Workspace)
                .WithMany(w => w.Bookings)
                .HasForeignKey(b => b.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ActivityLog -> User relationship
            modelBuilder.Entity<ActivityLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Workspace -> WorkspaceType relationship
            modelBuilder.Entity<Workspace>()
                .HasOne(w => w.WorkspaceType)
                .WithMany(t => t.Workspaces)
                .HasForeignKey(w => w.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure User -> Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint on Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Unique constraint on BookingCode
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingCode)
                .IsUnique();
        }
    }
}
