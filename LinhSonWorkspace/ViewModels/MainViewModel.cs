using System.Windows;
using System.Windows.Input;
using LinhSonWorkspace.Helpers;

namespace LinhSonWorkspace.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentView = null!;
        public BaseViewModel CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private string _currentUser = string.Empty;
        public string CurrentUserName
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        private string _currentRole = string.Empty;
        public string CurrentRole
        {
            get => _currentRole;
            set => SetProperty(ref _currentRole, value);
        }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get => _isAdmin;
            set => SetProperty(ref _isAdmin, value);
        }

        private string _selectedMenu = "Dashboard";
        public string SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                if (SetProperty(ref _selectedMenu, value))
                {
                    NavigateToView(value);
                }
            }
        }

        public ICommand NavigateCommand { get; }
        public ICommand LogoutCommand { get; }

        // Event to notify logout
        public event System.Action? LogoutRequested;

        public MainViewModel()
        {
            CurrentUserName = SessionHelper.CurrentUser?.FullName ?? "User";
            CurrentRole = SessionHelper.CurrentUser?.Role?.RoleName ?? "Staff";
            IsAdmin = SessionHelper.IsAdmin;

            NavigateCommand = new RelayCommand(param => SelectedMenu = param?.ToString() ?? "Dashboard");
            LogoutCommand = new RelayCommand(ExecuteLogout);

            // Start with Dashboard
            CurrentView = new DashboardViewModel();
        }

        private void NavigateToView(string viewName)
        {
            CurrentView = viewName switch
            {
                "Dashboard" => new DashboardViewModel(),
                "Workspaces" => new WorkspaceViewModel(),
                "Customers" => new CustomerViewModel(),
                "Bookings" => new BookingViewModel(),
                "CheckInOut" => new CheckInOutViewModel(),
                "Reports" => new ReportViewModel(),
                "Settings" => new SettingsViewModel(),
                _ => new DashboardViewModel()
            };
        }

        private void ExecuteLogout()
        {
            var result = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SessionHelper.Logout();
                LogoutRequested?.Invoke();
            }
        }
    }
}
