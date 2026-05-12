using System;
using System.Linq;
using LinhSonWorkspace.Models;
using BCrypt.Net;

namespace LinhSonWorkspace.Data
{
    /// <summary>
    /// Seeds initial data for development and demo purposes.
    /// All data is simulated for educational use only.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if data already seeded
            if (context.Roles.Any()) return;

            // ===== SEED ROLES =====
            var roles = new Role[]
            {
                new() { RoleName = "Admin" },
                new() { RoleName = "Staff" }
            };
            context.Roles.AddRange(roles);
            context.SaveChanges();

            // ===== SEED USERS =====
            var adminRole = context.Roles.First(r => r.RoleName == "Admin");
            var staffRole = context.Roles.First(r => r.RoleName == "Staff");

            var users = new User[]
            {
                new()
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Quản Trị Viên",
                    Email = "admin@linhsonworkspace.com",
                    RoleId = adminRole.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new()
                {
                    Username = "staff1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                    FullName = "Nguyễn Văn An",
                    Email = "an.nguyen@linhsonworkspace.com",
                    RoleId = staffRole.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new()
                {
                    Username = "staff2",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                    FullName = "Trần Thị Bình",
                    Email = "binh.tran@linhsonworkspace.com",
                    RoleId = staffRole.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // ===== SEED WORKSPACE TYPES =====
            var workspaceTypes = new WorkspaceType[]
            {
                new() { TypeName = "Hot Desk", Description = "Bàn làm việc cá nhân, linh hoạt theo giờ/ngày" },
                new() { TypeName = "Meeting Room", Description = "Phòng họp có bảng trắng, máy chiếu" },
                new() { TypeName = "Private Office", Description = "Phòng làm việc riêng, không gian yên tĩnh" },
                new() { TypeName = "Coworking Area", Description = "Khu vực làm việc chung, không gian mở" }
            };
            context.WorkspaceTypes.AddRange(workspaceTypes);
            context.SaveChanges();

            // ===== SEED WORKSPACES =====
            var hotDesk = context.WorkspaceTypes.First(t => t.TypeName == "Hot Desk");
            var meetingRoom = context.WorkspaceTypes.First(t => t.TypeName == "Meeting Room");
            var privateOffice = context.WorkspaceTypes.First(t => t.TypeName == "Private Office");
            var coworking = context.WorkspaceTypes.First(t => t.TypeName == "Coworking Area");

            var workspaces = new Workspace[]
            {
                // Hot Desks
                new() { Name = "Hot Desk A1", TypeId = hotDesk.TypeId, Capacity = 1, PricePerHour = 30000, PricePerDay = 200000, Status = "Available", Description = "Bàn làm việc cá nhân tầng 1, gần cửa sổ" },
                new() { Name = "Hot Desk A2", TypeId = hotDesk.TypeId, Capacity = 1, PricePerHour = 30000, PricePerDay = 200000, Status = "Available", Description = "Bàn làm việc cá nhân tầng 1" },
                new() { Name = "Hot Desk A3", TypeId = hotDesk.TypeId, Capacity = 1, PricePerHour = 30000, PricePerDay = 200000, Status = "Available", Description = "Bàn làm việc cá nhân tầng 1" },
                new() { Name = "Hot Desk B1", TypeId = hotDesk.TypeId, Capacity = 1, PricePerHour = 35000, PricePerDay = 230000, Status = "Available", Description = "Bàn làm việc cá nhân tầng 2, view đẹp" },
                new() { Name = "Hot Desk B2", TypeId = hotDesk.TypeId, Capacity = 1, PricePerHour = 35000, PricePerDay = 230000, Status = "Maintenance", Description = "Bàn làm việc cá nhân tầng 2 - đang bảo trì" },

                // Meeting Rooms
                new() { Name = "Meeting Room M1", TypeId = meetingRoom.TypeId, Capacity = 8, PricePerHour = 150000, PricePerDay = 1000000, Status = "Available", Description = "Phòng họp nhỏ, có bảng trắng và màn chiếu" },
                new() { Name = "Meeting Room M2", TypeId = meetingRoom.TypeId, Capacity = 12, PricePerHour = 200000, PricePerDay = 1400000, Status = "Available", Description = "Phòng họp lớn, có TV 65 inch và video call" },

                // Private Offices
                new() { Name = "Private Office P1", TypeId = privateOffice.TypeId, Capacity = 4, PricePerHour = 100000, PricePerDay = 700000, Status = "Available", Description = "Phòng làm việc riêng 4 người, yên tĩnh" },
                new() { Name = "Private Office P2", TypeId = privateOffice.TypeId, Capacity = 6, PricePerHour = 130000, PricePerDay = 900000, Status = "Available", Description = "Phòng làm việc riêng 6 người, view thành phố" },

                // Coworking
                new() { Name = "Coworking Zone C1", TypeId = coworking.TypeId, Capacity = 20, PricePerHour = 25000, PricePerDay = 170000, Status = "Available", Description = "Khu vực làm việc chung tầng 1, không gian mở" }
            };
            context.Workspaces.AddRange(workspaces);
            context.SaveChanges();

            // ===== SEED CUSTOMERS =====
            var customers = new Customer[]
            {
                new() { FullName = "Lê Minh Tuấn", Phone = "0901234567", Email = "tuan.le@gmail.com", Note = "Khách thường xuyên", CreatedAt = DateTime.Now.AddDays(-30) },
                new() { FullName = "Phạm Thu Hà", Phone = "0912345678", Email = "ha.pham@gmail.com", Note = "Startup team leader", CreatedAt = DateTime.Now.AddDays(-25) },
                new() { FullName = "Nguyễn Đức Anh", Phone = "0923456789", Email = "anh.nguyen@company.com", Note = "Freelancer", CreatedAt = DateTime.Now.AddDays(-20) },
                new() { FullName = "Trần Văn Hùng", Phone = "0934567890", Email = "hung.tran@business.com", Note = "Công ty ABC", CreatedAt = DateTime.Now.AddDays(-15) },
                new() { FullName = "Vũ Thị Lan", Phone = "0945678901", Email = "lan.vu@email.com", Note = "", CreatedAt = DateTime.Now.AddDays(-10) }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();

            // ===== SEED SAMPLE BOOKINGS =====
            var staff1 = context.Users.First(u => u.Username == "staff1");

            var bookings = new Booking[]
            {
                new()
                {
                    BookingCode = "BK001",
                    CustomerId = customers[0].CustomerId,
                    WorkspaceId = workspaces[0].WorkspaceId,
                    StartTime = DateTime.Today.AddHours(8),
                    EndTime = DateTime.Today.AddHours(12),
                    TotalPrice = 120000,
                    Status = "Confirmed",
                    CreatedBy = staff1.UserId,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Note = "Khách đặt trước 1 ngày"
                },
                new()
                {
                    BookingCode = "BK002",
                    CustomerId = customers[1].CustomerId,
                    WorkspaceId = workspaces[5].WorkspaceId,
                    StartTime = DateTime.Today.AddHours(14),
                    EndTime = DateTime.Today.AddHours(16),
                    TotalPrice = 300000,
                    Status = "Confirmed",
                    CreatedBy = staff1.UserId,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Note = "Họp nhóm startup"
                },
                new()
                {
                    BookingCode = "BK003",
                    CustomerId = customers[2].CustomerId,
                    WorkspaceId = workspaces[7].WorkspaceId,
                    StartTime = DateTime.Today.AddDays(1).AddHours(9),
                    EndTime = DateTime.Today.AddDays(1).AddHours(17),
                    TotalPrice = 700000,
                    Status = "Pending",
                    CreatedBy = staff1.UserId,
                    CreatedAt = DateTime.Now,
                    Note = "Làm việc cả ngày"
                },
                new()
                {
                    BookingCode = "BK004",
                    CustomerId = customers[3].CustomerId,
                    WorkspaceId = workspaces[6].WorkspaceId,
                    StartTime = DateTime.Today.AddDays(-2).AddHours(10),
                    EndTime = DateTime.Today.AddDays(-2).AddHours(12),
                    TotalPrice = 400000,
                    Status = "Completed",
                    CreatedBy = staff1.UserId,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    Note = "Họp với đối tác"
                },
                new()
                {
                    BookingCode = "BK005",
                    CustomerId = customers[0].CustomerId,
                    WorkspaceId = workspaces[3].WorkspaceId,
                    StartTime = DateTime.Today.AddDays(-1).AddHours(8),
                    EndTime = DateTime.Today.AddDays(-1).AddHours(17),
                    TotalPrice = 230000,
                    Status = "Completed",
                    CreatedBy = staff1.UserId,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    Note = ""
                }
            };
            context.Bookings.AddRange(bookings);
            context.SaveChanges();
        }
    }
}
