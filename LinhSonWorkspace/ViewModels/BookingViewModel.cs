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
using Microsoft.Win32;

namespace LinhSonWorkspace.ViewModels
{
    public class BookingViewModel : BaseViewModel
    {
        private readonly BookingService _bookingService = new();
        private readonly ExportService _exportService = new();
        private readonly LogService _logService = new();

        private ObservableCollection<Booking> _bookings = new();
        public ObservableCollection<Booking> Bookings { get => _bookings; set => SetProperty(ref _bookings, value); }

        private Booking? _selectedBooking;
        public Booking? SelectedBooking { get => _selectedBooking; set => SetProperty(ref _selectedBooking, value); }

        // Filters
        private string _filterStatus = "All";
        public string FilterStatus { get => _filterStatus; set { if (SetProperty(ref _filterStatus, value)) _ = LoadDataAsync(); } }
        private DateTime? _filterDateFrom;
        public DateTime? FilterDateFrom { get => _filterDateFrom; set { if (SetProperty(ref _filterDateFrom, value)) _ = LoadDataAsync(); } }
        private DateTime? _filterDateTo;
        public DateTime? FilterDateTo { get => _filterDateTo; set { if (SetProperty(ref _filterDateTo, value)) _ = LoadDataAsync(); } }
        private string _searchText = "";
        public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) _ = LoadDataAsync(); } }

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Confirmed", "CheckedIn", "Completed", "Cancelled" };

        // Create booking form
        private bool _isCreating;
        public bool IsCreating { get => _isCreating; set => SetProperty(ref _isCreating, value); }
        private ObservableCollection<Customer> _allCustomers = new();
        public ObservableCollection<Customer> AllCustomers { get => _allCustomers; set => SetProperty(ref _allCustomers, value); }
        private ObservableCollection<Workspace> _availableWorkspaces = new();
        public ObservableCollection<Workspace> AvailableWorkspaces { get => _availableWorkspaces; set => SetProperty(ref _availableWorkspaces, value); }

        private Customer? _newCustomer;
        public Customer? NewCustomer { get => _newCustomer; set => SetProperty(ref _newCustomer, value); }
        private Workspace? _newWorkspace;
        public Workspace? NewWorkspace { get => _newWorkspace; set { if (SetProperty(ref _newWorkspace, value)) RecalculatePrice(); } }
        private DateTime _newStartDate = DateTime.Today;
        public DateTime NewStartDate { get => _newStartDate; set { if (SetProperty(ref _newStartDate, value)) RecalculatePrice(); } }
        private string _newStartHour = "08";
        public string NewStartHour { get => _newStartHour; set { if (SetProperty(ref _newStartHour, value)) RecalculatePrice(); } }
        private string _newStartMinute = "00";
        public string NewStartMinute { get => _newStartMinute; set { if (SetProperty(ref _newStartMinute, value)) RecalculatePrice(); } }
        private DateTime _newEndDate = DateTime.Today;
        public DateTime NewEndDate { get => _newEndDate; set { if (SetProperty(ref _newEndDate, value)) RecalculatePrice(); } }
        private string _newEndHour = "12";
        public string NewEndHour { get => _newEndHour; set { if (SetProperty(ref _newEndHour, value)) RecalculatePrice(); } }
        private string _newEndMinute = "00";
        public string NewEndMinute { get => _newEndMinute; set { if (SetProperty(ref _newEndMinute, value)) RecalculatePrice(); } }
        private string _newNote = "";
        public string NewNote { get => _newNote; set => SetProperty(ref _newNote, value); }
        private decimal _calculatedPrice;
        public decimal CalculatedPrice { get => _calculatedPrice; set => SetProperty(ref _calculatedPrice, value); }

        public ICommand RefreshCommand { get; }
        public ICommand CreateBookingCommand { get; }
        public ICommand SaveBookingCommand { get; }
        public ICommand CancelCreateCommand { get; }
        public ICommand ConfirmBookingCommand { get; }
        public ICommand CancelBookingCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand ExportJsonCommand { get; }

        public BookingViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            CreateBookingCommand = new AsyncRelayCommand(StartCreateAsync);
            SaveBookingCommand = new AsyncRelayCommand(SaveBookingAsync);
            CancelCreateCommand = new RelayCommand(() => IsCreating = false);
            ConfirmBookingCommand = new AsyncRelayCommand(ConfirmBookingAsync);
            CancelBookingCommand = new AsyncRelayCommand(CancelBookingAsync);
            ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync);
            ExportJsonCommand = new AsyncRelayCommand(ExportJsonAsync);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            using var context = new AppDbContext();
            var query = context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Workspace)
                .Include(b => b.CreatedByUser)
                .AsQueryable();

            if (FilterStatus != "All")
                query = query.Where(b => b.Status == FilterStatus);
            if (FilterDateFrom.HasValue)
                query = query.Where(b => b.StartTime.Date >= FilterDateFrom.Value.Date);
            if (FilterDateTo.HasValue)
                query = query.Where(b => b.StartTime.Date <= FilterDateTo.Value.Date);
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(b => b.BookingCode.Contains(SearchText) || b.Customer!.FullName.Contains(SearchText) || b.Workspace!.Name.Contains(SearchText));

            Bookings = new ObservableCollection<Booking>(await query.OrderByDescending(b => b.CreatedAt).ToListAsync());
        }

        private async Task StartCreateAsync()
        {
            using var context = new AppDbContext();
            AllCustomers = new ObservableCollection<Customer>(await context.Customers.OrderBy(c => c.FullName).ToListAsync());
            AvailableWorkspaces = new ObservableCollection<Workspace>(await context.Workspaces.Where(w => w.Status == "Available").Include(w => w.WorkspaceType).OrderBy(w => w.Name).ToListAsync());
            NewCustomer = null; NewWorkspace = null; NewNote = "";
            NewStartDate = DateTime.Today; NewEndDate = DateTime.Today;
            NewStartHour = "08"; NewStartMinute = "00"; NewEndHour = "12"; NewEndMinute = "00";
            CalculatedPrice = 0;
            IsCreating = true;
        }

        private void RecalculatePrice()
        {
            if (NewWorkspace == null) { CalculatedPrice = 0; return; }
            try
            {
                var start = NewStartDate.Date.AddHours(int.Parse(NewStartHour)).AddMinutes(int.Parse(NewStartMinute));
                var end = NewEndDate.Date.AddHours(int.Parse(NewEndHour)).AddMinutes(int.Parse(NewEndMinute));
                if (end > start) CalculatedPrice = _bookingService.CalculatePrice(NewWorkspace, start, end);
                else CalculatedPrice = 0;
            }
            catch { CalculatedPrice = 0; }
        }

        private async Task SaveBookingAsync()
        {
            if (NewCustomer == null) { MessageBox.Show("Vui lòng chọn khách hàng."); return; }
            if (NewWorkspace == null) { MessageBox.Show("Vui lòng chọn workspace."); return; }

            var start = NewStartDate.Date.AddHours(int.Parse(NewStartHour)).AddMinutes(int.Parse(NewStartMinute));
            var end = NewEndDate.Date.AddHours(int.Parse(NewEndHour)).AddMinutes(int.Parse(NewEndMinute));
            if (end <= start) { MessageBox.Show("Thời gian kết thúc phải sau thời gian bắt đầu."); return; }

            try
            {
                var booking = await _bookingService.CreateBookingAsync(
                    NewCustomer.CustomerId, NewWorkspace.WorkspaceId, start, end, NewNote, SessionHelper.CurrentUser!.UserId);

                await _logService.LogActivityAsync("CREATE_BOOKING",
                    $"Staff {SessionHelper.CurrentUser.FullName} created booking #{booking.BookingCode} for {NewWorkspace.Name}");

                MessageBox.Show($"Tạo booking thành công! Mã: {booking.BookingCode}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                IsCreating = false;
                await LoadDataAsync();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Lỗi đặt lịch", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task ConfirmBookingAsync()
        {
            if (SelectedBooking == null || SelectedBooking.Status != "Pending") { MessageBox.Show("Vui lòng chọn booking Pending."); return; }
            await _bookingService.UpdateBookingStatusAsync(SelectedBooking.BookingId, "Confirmed");
            await _logService.LogActivityAsync("CONFIRM_BOOKING", $"Confirmed booking #{SelectedBooking.BookingCode}");
            await LoadDataAsync();
        }

        private async Task CancelBookingAsync()
        {
            if (SelectedBooking == null) { MessageBox.Show("Vui lòng chọn booking."); return; }
            if (SelectedBooking.Status == "Completed" || SelectedBooking.Status == "Cancelled") { MessageBox.Show("Không thể hủy booking này."); return; }
            if (MessageBox.Show("Hủy booking này?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            await _bookingService.UpdateBookingStatusAsync(SelectedBooking.BookingId, "Cancelled");
            await _logService.LogActivityAsync("CANCEL_BOOKING", $"Cancelled booking #{SelectedBooking.BookingCode}");
            await LoadDataAsync();
        }

        private async Task ExportCsvAsync()
        {
            var dlg = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = $"bookings_{DateTime.Now:yyyyMMdd}.csv" };
            if (dlg.ShowDialog() != true) return;
            await _exportService.ExportBookingsToCsvAsync(Bookings, dlg.FileName);
            await _logService.LogActivityAsync("EXPORT_CSV", $"Exported {Bookings.Count} bookings to CSV");
            MessageBox.Show("Export CSV thành công!", "Thành công");
        }

        private async Task ExportJsonAsync()
        {
            var dlg = new SaveFileDialog { Filter = "JSON Files|*.json", FileName = $"bookings_{DateTime.Now:yyyyMMdd}.json" };
            if (dlg.ShowDialog() != true) return;
            await _exportService.ExportBookingsToJsonAsync(Bookings, dlg.FileName);
            await _logService.LogActivityAsync("EXPORT_JSON", $"Exported {Bookings.Count} bookings to JSON");
            MessageBox.Show("Export JSON thành công!", "Thành công");
        }
    }
}
