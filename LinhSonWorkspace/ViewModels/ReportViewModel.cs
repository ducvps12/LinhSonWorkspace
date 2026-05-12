using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LinhSonWorkspace.Data;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace LinhSonWorkspace.ViewModels
{
    public class ReportViewModel : BaseViewModel
    {
        private DateTime _dateFrom = DateTime.Today.AddDays(-30);
        public DateTime DateFrom { get => _dateFrom; set { if (SetProperty(ref _dateFrom, value)) _ = LoadDataAsync(); } }

        private DateTime _dateTo = DateTime.Today;
        public DateTime DateTo { get => _dateTo; set { if (SetProperty(ref _dateTo, value)) _ = LoadDataAsync(); } }

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }

        private int _totalBookings;
        public int TotalBookings { get => _totalBookings; set => SetProperty(ref _totalBookings, value); }

        private int _completedBookings;
        public int CompletedBookings { get => _completedBookings; set => SetProperty(ref _completedBookings, value); }

        private int _cancelledBookings;
        public int CancelledBookings { get => _cancelledBookings; set => SetProperty(ref _cancelledBookings, value); }

        private string _topWorkspace = "";
        public string TopWorkspace { get => _topWorkspace; set => SetProperty(ref _topWorkspace, value); }

        // Charts
        private ISeries[] _revenueSeries = Array.Empty<ISeries>();
        public ISeries[] RevenueSeries { get => _revenueSeries; set => SetProperty(ref _revenueSeries, value); }

        private Axis[] _revenueXAxes = Array.Empty<Axis>();
        public Axis[] RevenueXAxes { get => _revenueXAxes; set => SetProperty(ref _revenueXAxes, value); }

        private ISeries[] _bookingStatusSeries = Array.Empty<ISeries>();
        public ISeries[] BookingStatusSeries { get => _bookingStatusSeries; set => SetProperty(ref _bookingStatusSeries, value); }

        public ICommand RefreshCommand { get; }

        public ReportViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var context = new AppDbContext();

                var bookings = await context.Bookings
                    .Include(b => b.Workspace)
                    .Where(b => b.StartTime.Date >= DateFrom.Date && b.StartTime.Date <= DateTo.Date)
                    .ToListAsync();

                TotalBookings = bookings.Count;
                CompletedBookings = bookings.Count(b => b.Status == "Completed");
                CancelledBookings = bookings.Count(b => b.Status == "Cancelled");
                TotalRevenue = bookings.Where(b => b.Status == "Completed" || b.Status == "CheckedIn").Sum(b => b.TotalPrice);

                // Top workspace
                var topWs = bookings.Where(b => b.Status != "Cancelled")
                    .GroupBy(b => b.Workspace?.Name ?? "Unknown")
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                TopWorkspace = topWs != null ? $"{topWs.Key} ({topWs.Count()} lượt)" : "N/A";

                // Revenue chart by day
                var revenueByDay = bookings
                    .Where(b => b.Status == "Completed" || b.Status == "CheckedIn")
                    .GroupBy(b => b.StartTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new { Date = g.Key, Revenue = (double)g.Sum(b => b.TotalPrice) })
                    .ToList();

                RevenueSeries = new ISeries[]
                {
                    new ColumnSeries<double>
                    {
                        Values = revenueByDay.Select(r => r.Revenue).ToArray(),
                        Name = "Doanh thu (VNĐ)",
                        Fill = new SolidColorPaint(new SKColor(0, 180, 216))
                    }
                };

                RevenueXAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = revenueByDay.Select(r => r.Date.ToString("dd/MM")).ToArray(),
                        LabelsRotation = 45
                    }
                };

                // Booking status pie chart
                var statusGroups = bookings.GroupBy(b => b.Status).ToList();
                var colors = new SKColor[] {
                    new(245, 158, 11), new(59, 130, 246), new(139, 92, 246),
                    new(16, 185, 129), new(239, 68, 68) };
                int ci = 0;

                BookingStatusSeries = statusGroups.Select(g => (ISeries)new PieSeries<int>
                {
                    Values = new[] { g.Count() },
                    Name = g.Key,
                    Fill = new SolidColorPaint(colors[ci++ % colors.Length])
                }).ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải báo cáo: {ex.Message}");
            }
        }
    }
}
