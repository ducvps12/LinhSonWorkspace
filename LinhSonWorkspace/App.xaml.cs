using System.Windows;
using LinhSonWorkspace.Data;

namespace LinhSonWorkspace
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize database and seed data
            try
            {
                using var context = new AppDbContext();
                DbInitializer.Initialize(context);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo database: {ex.Message}\n\nVui lòng kiểm tra SQL Server LocalDB.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
