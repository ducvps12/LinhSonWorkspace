using System.Windows;
using LinhSonWorkspace.Data;

namespace LinhSonWorkspace
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Nếu vẫn bị đen màn hình, hãy bỏ comment dòng dưới đây để tắt Hardware Acceleration:
            // System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            // Initialize database and seed data
            try
            {
                await System.Threading.Tasks.Task.Run(() => 
                {
                    using var context = new AppDbContext();
                    DbInitializer.Initialize(context);
                });
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo database: {ex.Message}\n\nVui lòng kiểm tra SQL Server LocalDB.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
