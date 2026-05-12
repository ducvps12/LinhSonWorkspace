using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Helpers;
using LinhSonWorkspace.Models;
using LinhSonWorkspace.Services;
using Microsoft.EntityFrameworkCore;

namespace LinhSonWorkspace.ViewModels
{
    public class CheckInOutViewModel : BaseViewModel
    {
        private readonly BookingService _bookingService = new();
        private readonly LogService _logService = new();

        private ObservableCollection<Booking> _todayBookings = new();
        public ObservableCollection<Booking> TodayBookings { get => _todayBookings; set => SetProperty(ref _todayBookings, value); }

        private Booking? _selectedBooking;
        public Booking? SelectedBooking { get => _selectedBooking; set => SetProperty(ref _selectedBooking, value); }

        private string _filterStatus = "All";
        public string FilterStatus { get => _filterStatus; set { if (SetProperty(ref _filterStatus, value)) _ = LoadDataAsync(); } }

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Confirmed", "CheckedIn" };

        public ICommand RefreshCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand CheckOutCommand { get; }

        public CheckInOutViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            CheckInCommand = new AsyncRelayCommand(CheckInAsync);
            CheckOutCommand = new AsyncRelayCommand(CheckOutAsync);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            using var context = new AppDbContext();
            var today = DateTime.Today;
            var query = context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Workspace)
                .Where(b => b.StartTime.Date == today && b.Status != "Cancelled" && b.Status != "Pending")
                .AsQueryable();

            if (FilterStatus != "All")
                query = query.Where(b => b.Status == FilterStatus);

            TodayBookings = new ObservableCollection<Booking>(await query.OrderBy(b => b.StartTime).ToListAsync());
        }

        private async Task CheckInAsync()
        {
            if (SelectedBooking == null) { MessageBox.Show("Vui lòng chọn booking."); return; }
            if (SelectedBooking.Status != "Confirmed") { MessageBox.Show("Chỉ có thể check-in booking đã Confirmed."); return; }

            await _bookingService.UpdateBookingStatusAsync(SelectedBooking.BookingId, "CheckedIn");
            await _logService.LogActivityAsync("CHECK_IN",
                $"Staff {SessionHelper.CurrentUser?.FullName} checked-in booking #{SelectedBooking.BookingCode}");
            MessageBox.Show($"Check-in thành công cho booking #{SelectedBooking.BookingCode}!", "Thành công");
            await LoadDataAsync();
        }

        private async Task CheckOutAsync()
        {
            if (SelectedBooking == null) { MessageBox.Show("Vui lòng chọn booking."); return; }
            if (SelectedBooking.Status != "CheckedIn") { MessageBox.Show("Chỉ có thể check-out booking đang CheckedIn."); return; }

            await _bookingService.UpdateBookingStatusAsync(SelectedBooking.BookingId, "Completed");
            await _logService.LogActivityAsync("CHECK_OUT",
                $"Staff {SessionHelper.CurrentUser?.FullName} checked-out booking #{SelectedBooking.BookingCode}");
            MessageBox.Show($"Check-out thành công cho booking #{SelectedBooking.BookingCode}!", "Thành công");
            await LoadDataAsync();
        }
    }
}
