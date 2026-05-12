using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Models;
using Microsoft.EntityFrameworkCore;

namespace LinhSonWorkspace.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private int _todayBookings;
        public int TodayBookings
        {
            get => _todayBookings;
            set => SetProperty(ref _todayBookings, value);
        }

        private int _activeWorkspaces;
        public int ActiveWorkspaces
        {
            get => _activeWorkspaces;
            set => SetProperty(ref _activeWorkspaces, value);
        }

        private decimal _todayRevenue;
        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set => SetProperty(ref _todayRevenue, value);
        }

        private int _totalCustomers;
        public int TotalCustomers
        {
            get => _totalCustomers;
            set => SetProperty(ref _totalCustomers, value);
        }

        private ObservableCollection<Booking> _upcomingBookings = new();
        public ObservableCollection<Booking> UpcomingBookings
        {
            get => _upcomingBookings;
            set => SetProperty(ref _upcomingBookings, value);
        }

        private ObservableCollection<Booking> _recentBookings = new();
        public ObservableCollection<Booking> RecentBookings
        {
            get => _recentBookings;
            set => SetProperty(ref _recentBookings, value);
        }

        public ICommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var context = new AppDbContext();
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // Today's bookings count
                TodayBookings = await context.Bookings
                    .CountAsync(b => b.StartTime.Date == today && b.Status != "Cancelled");

                // Active workspaces (currently in use - CheckedIn status)
                ActiveWorkspaces = await context.Bookings
                    .CountAsync(b => b.Status == "CheckedIn");

                // Today's revenue (Completed bookings today)
                TodayRevenue = await context.Bookings
                    .Where(b => b.StartTime.Date == today && (b.Status == "Completed" || b.Status == "CheckedIn"))
                    .SumAsync(b => b.TotalPrice);

                // Total customers
                TotalCustomers = await context.Customers.CountAsync();

                // Upcoming bookings (Confirmed, starting from now)
                var upcoming = await context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Workspace)
                    .Where(b => b.StartTime >= DateTime.Now && (b.Status == "Confirmed" || b.Status == "Pending"))
                    .OrderBy(b => b.StartTime)
                    .Take(5)
                    .ToListAsync();

                UpcomingBookings = new ObservableCollection<Booking>(upcoming);

                // Recent bookings (last 5)
                var recent = await context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Workspace)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                RecentBookings = new ObservableCollection<Booking>(recent);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
