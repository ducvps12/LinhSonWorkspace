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
    public class WorkspaceViewModel : BaseViewModel
    {
        private readonly LogService _logService = new();

        private ObservableCollection<Workspace> _workspaces = new();
        public ObservableCollection<Workspace> Workspaces { get => _workspaces; set => SetProperty(ref _workspaces, value); }

        private Workspace? _selectedWorkspace;
        public Workspace? SelectedWorkspace { get => _selectedWorkspace; set => SetProperty(ref _selectedWorkspace, value); }

        private string _searchText = "";
        public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) _ = LoadDataAsync(); } }

        private string _filterType = "All";
        public string FilterType { get => _filterType; set { if (SetProperty(ref _filterType, value)) _ = LoadDataAsync(); } }

        private string _filterStatus = "All";
        public string FilterStatus { get => _filterStatus; set { if (SetProperty(ref _filterStatus, value)) _ = LoadDataAsync(); } }

        private ObservableCollection<string> _workspaceTypes = new() { "All" };
        public ObservableCollection<string> WorkspaceTypes { get => _workspaceTypes; set => SetProperty(ref _workspaceTypes, value); }

        public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Available", "Maintenance", "Inactive" };

        private bool _isEditing;
        public bool IsEditing { get => _isEditing; set => SetProperty(ref _isEditing, value); }
        private bool _isAddMode;
        public bool IsAddMode { get => _isAddMode; set => SetProperty(ref _isAddMode, value); }

        private string _editName = "";
        public string EditName { get => _editName; set => SetProperty(ref _editName, value); }
        private int _editTypeId;
        public int EditTypeId { get => _editTypeId; set => SetProperty(ref _editTypeId, value); }
        private int _editCapacity = 1;
        public int EditCapacity { get => _editCapacity; set => SetProperty(ref _editCapacity, value); }
        private decimal _editPricePerHour;
        public decimal EditPricePerHour { get => _editPricePerHour; set => SetProperty(ref _editPricePerHour, value); }
        private decimal _editPricePerDay;
        public decimal EditPricePerDay { get => _editPricePerDay; set => SetProperty(ref _editPricePerDay, value); }
        private string _editStatus = "Available";
        public string EditStatus { get => _editStatus; set => SetProperty(ref _editStatus, value); }
        private string _editDescription = "";
        public string EditDescription { get => _editDescription; set => SetProperty(ref _editDescription, value); }

        private ObservableCollection<WorkspaceType> _allTypes = new();
        public ObservableCollection<WorkspaceType> AllTypes { get => _allTypes; set => SetProperty(ref _allTypes, value); }

        public bool IsAdmin => SessionHelper.IsAdmin;

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelEditCommand { get; }

        public WorkspaceViewModel()
        {
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            AddCommand = new RelayCommand(StartAdd);
            EditCommand = new RelayCommand(StartEdit);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelEditCommand = new RelayCommand(() => IsEditing = false);
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            using var context = new AppDbContext();
            var types = await context.WorkspaceTypes.ToListAsync();
            AllTypes = new ObservableCollection<WorkspaceType>(types);
            var typeNames = new ObservableCollection<string> { "All" };
            foreach (var t in types) typeNames.Add(t.TypeName);
            WorkspaceTypes = typeNames;

            var query = context.Workspaces.Include(w => w.WorkspaceType).AsQueryable();
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(w => w.Name.Contains(SearchText) || w.Description.Contains(SearchText));
            if (FilterType != "All")
                query = query.Where(w => w.WorkspaceType!.TypeName == FilterType);
            if (FilterStatus != "All")
                query = query.Where(w => w.Status == FilterStatus);

            Workspaces = new ObservableCollection<Workspace>(await query.OrderBy(w => w.Name).ToListAsync());
        }

        private void StartAdd()
        {
            if (!SessionHelper.IsAdmin) { MessageBox.Show("Chỉ Admin mới có quyền thêm workspace."); return; }
            IsAddMode = true; IsEditing = true;
            EditName = ""; EditTypeId = AllTypes.FirstOrDefault()?.TypeId ?? 0;
            EditCapacity = 1; EditPricePerHour = 0; EditPricePerDay = 0;
            EditStatus = "Available"; EditDescription = "";
        }

        private void StartEdit()
        {
            if (!SessionHelper.IsAdmin) { MessageBox.Show("Chỉ Admin mới có quyền sửa workspace."); return; }
            if (SelectedWorkspace == null) { MessageBox.Show("Vui lòng chọn workspace."); return; }
            IsAddMode = false; IsEditing = true;
            EditName = SelectedWorkspace.Name; EditTypeId = SelectedWorkspace.TypeId;
            EditCapacity = SelectedWorkspace.Capacity; EditPricePerHour = SelectedWorkspace.PricePerHour;
            EditPricePerDay = SelectedWorkspace.PricePerDay; EditStatus = SelectedWorkspace.Status;
            EditDescription = SelectedWorkspace.Description;
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(EditName)) { MessageBox.Show("Vui lòng nhập tên workspace."); return; }
            using var context = new AppDbContext();
            if (IsAddMode)
            {
                var ws = new Workspace { Name = EditName, TypeId = EditTypeId, Capacity = EditCapacity, PricePerHour = EditPricePerHour, PricePerDay = EditPricePerDay, Status = EditStatus, Description = EditDescription };
                context.Workspaces.Add(ws); await context.SaveChangesAsync();
                await _logService.LogActivityAsync("CREATE_WORKSPACE", $"Created workspace: {ws.Name}");
            }
            else if (SelectedWorkspace != null)
            {
                var ws = await context.Workspaces.FindAsync(SelectedWorkspace.WorkspaceId);
                if (ws != null) { ws.Name = EditName; ws.TypeId = EditTypeId; ws.Capacity = EditCapacity; ws.PricePerHour = EditPricePerHour; ws.PricePerDay = EditPricePerDay; ws.Status = EditStatus; ws.Description = EditDescription; await context.SaveChangesAsync(); }
                await _logService.LogActivityAsync("UPDATE_WORKSPACE", $"Updated workspace: {EditName}");
            }
            IsEditing = false; await LoadDataAsync();
        }

        private async Task DeleteAsync()
        {
            if (!SessionHelper.IsAdmin) { MessageBox.Show("Chỉ Admin mới có quyền xóa workspace."); return; }
            if (SelectedWorkspace == null) { MessageBox.Show("Vui lòng chọn workspace."); return; }
            if (MessageBox.Show($"Xóa workspace '{SelectedWorkspace.Name}'?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            using var context = new AppDbContext();
            if (await context.Bookings.AnyAsync(b => b.WorkspaceId == SelectedWorkspace.WorkspaceId))
            { MessageBox.Show("Không thể xóa workspace đã có booking. Hãy chuyển sang Inactive."); return; }
            var ws = await context.Workspaces.FindAsync(SelectedWorkspace.WorkspaceId);
            if (ws != null) { context.Workspaces.Remove(ws); await context.SaveChangesAsync(); }
            await _logService.LogActivityAsync("DELETE_WORKSPACE", $"Deleted workspace: {SelectedWorkspace.Name}");
            await LoadDataAsync();
        }
    }
}
