using System.Windows;
using LinhSonWorkspace.ViewModels;
using LinhSonWorkspace.Views;

namespace LinhSonWorkspace
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = (MainViewModel)DataContext;
            vm.LogoutRequested += OnLogoutRequested;
        }

        private void OnLogoutRequested()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}