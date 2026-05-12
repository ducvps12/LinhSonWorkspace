using System.Windows;
using System.Windows.Controls;
using LinhSonWorkspace.ViewModels;

namespace LinhSonWorkspace.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var vm = (LoginViewModel)DataContext;
            vm.LoginSucceeded += OnLoginSucceeded;

            // Bind PasswordBox manually (WPF PasswordBox doesn't support binding for security)
            txtPassword.PasswordChanged += (s, e) => vm.Password = txtPassword.Password;
        }

        private void OnLoginSucceeded()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
