using System.Windows;
using System.Windows.Input;
using LinhSonWorkspace.Helpers;
using LinhSonWorkspace.Services;

namespace LinhSonWorkspace.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService = new();

        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }

        // Event to notify successful login
        public event System.Action? LoginSucceeded;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, () => !IsLoading);
        }

        private void ExecuteLogin()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Vui lòng nhập tên đăng nhập";
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu";
                return;
            }

            IsLoading = true;

            var user = _authService.Login(Username, Password);

            if (user != null)
            {
                SessionHelper.CurrentUser = user;
                LoginSucceeded?.Invoke();
            }
            else
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng";
            }

            IsLoading = false;
        }
    }
}
