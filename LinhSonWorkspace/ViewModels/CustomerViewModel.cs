using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Models;
using LinhSonWorkspace.Services;
using Microsoft.EntityFrameworkCore;

namespace LinhSonWorkspace.ViewModels
{
    public class CustomerViewModel : BaseViewModel
    {
        private readonly LogService _logService = new();
        private ObservableCollection<Customer> _customers = new();
        public ObservableCollection<Customer> Customers { get => _customers; set => SetProperty(ref _customers, value); }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer { get => _selectedCustomer; set => SetProperty(ref _selectedCustomer, value); }

        private string _searchText = string.Empty;
        public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) _ = LoadDataAsync(); } }

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }
        private bool _isAddMode;
        public bool IsAddMode { get => _isAddMode; set => SetProperty(ref _isAddMode, value); }

        private string _editName = "";
        public string EditName { get => _editName; set => SetProperty(ref _editName, value); }
        private string _editPhone = "";
        public string EditPhone { get => _editPhone; set => SetProperty(ref _editPhone, value); }
        private string _editEmail = "";
        public string EditEmail { get => _editEmail; set => SetProperty(ref _editEmail, value); }
        private string _editNote = "";
        public string EditNote { get => _editNote; set => SetProperty(ref _editNote, value); }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelEditCommand { get; }

        public CustomerViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            AddCommand = new RelayCommand(() => { IsAddMode = true; IsEditing = true; EditName = ""; EditPhone = ""; EditEmail = ""; EditNote = ""; });
            EditCommand = new RelayCommand(StartEdit);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelEditCommand = new RelayCommand(() => IsEditing = false);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            using var context = new AppDbContext();
            var query = context.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(c => c.FullName.Contains(SearchText) || c.Phone.Contains(SearchText) || c.Email.Contains(SearchText));
            Customers = new ObservableCollection<Customer>(await query.OrderBy(c => c.FullName).ToListAsync());
        }

        private void StartEdit()
        {
            if (SelectedCustomer == null) { MessageBox.Show("Vui lòng chọn khách hàng."); return; }
            IsAddMode = false; IsEditing = true;
            EditName = SelectedCustomer.FullName; EditPhone = SelectedCustomer.Phone;
            EditEmail = SelectedCustomer.Email; EditNote = SelectedCustomer.Note;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(EditName)) { MessageBox.Show("Vui lòng nhập tên khách hàng."); return; }
            using var context = new AppDbContext();
            if (IsAddMode)
            {
                var c = new Customer { FullName = EditName, Phone = EditPhone, Email = EditEmail, Note = EditNote, CreatedAt = DateTime.Now };
                context.Customers.Add(c);
                await context.SaveChangesAsync();
                await _logService.LogActivityAsync("CREATE_CUSTOMER", $"Created customer: {c.FullName}");
            }
            else if (SelectedCustomer != null)
            {
                var c = await context.Customers.FindAsync(SelectedCustomer.CustomerId);
                if (c != null) { c.FullName = EditName; c.Phone = EditPhone; c.Email = EditEmail; c.Note = EditNote; await context.SaveChangesAsync(); }
                await _logService.LogActivityAsync("UPDATE_CUSTOMER", $"Updated customer: {EditName}");
            }
            IsEditing = false;
            await LoadDataAsync();
        }

        private async Task DeleteAsync()
        {
            if (SelectedCustomer == null) { MessageBox.Show("Vui lòng chọn khách hàng."); return; }
            if (MessageBox.Show($"Xóa khách hàng '{SelectedCustomer.FullName}'?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            using var context = new AppDbContext();
            if (await context.Bookings.AnyAsync(b => b.CustomerId == SelectedCustomer.CustomerId))
            { MessageBox.Show("Không thể xóa khách hàng đã có booking."); return; }
            var c = await context.Customers.FindAsync(SelectedCustomer.CustomerId);
            if (c != null) { context.Customers.Remove(c); await context.SaveChangesAsync(); }
            await _logService.LogActivityAsync("DELETE_CUSTOMER", $"Deleted customer: {SelectedCustomer.FullName}");
            await LoadDataAsync();
        }
    }
}
