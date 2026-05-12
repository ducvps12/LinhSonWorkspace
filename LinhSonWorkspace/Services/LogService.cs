using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Helpers;
using LinhSonWorkspace.Models;

namespace LinhSonWorkspace.Services
{
    /// <summary>
    /// Handles activity logging to both database and text file.
    /// Demonstrates Stream I/O for PRN212.
    /// </summary>
    public class LogService
    {
        private static readonly string LogDirectory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Logs");

        private static readonly string LogFilePath = Path.Combine(
            LogDirectory, $"activity_log_{DateTime.Now:yyyy-MM-dd}.txt");

        /// <summary>
        /// Logs an activity to both the database and a text file.
        /// </summary>
        public async Task LogActivityAsync(string action, string detail)
        {
            // Log to database
            await LogToDatabaseAsync(action, detail);

            // Log to text file (Stream I/O)
            await LogToFileAsync(action, detail);
        }

        /// <summary>
        /// Saves activity log to database via EF Core.
        /// </summary>
        private async Task LogToDatabaseAsync(string action, string detail)
        {
            using var context = new AppDbContext();

            var log = new ActivityLog
            {
                UserId = SessionHelper.CurrentUser?.UserId ?? 0,
                Action = action,
                Detail = detail,
                Timestamp = DateTime.Now
            };

            context.ActivityLogs.Add(log);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Appends activity log to a text file using StreamWriter.
        /// Format: [yyyy-MM-dd HH:mm:ss] Username - Action: Detail
        /// </summary>
        private async Task LogToFileAsync(string action, string detail)
        {
            // Ensure log directory exists
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            var username = SessionHelper.CurrentUser?.FullName ?? "System";
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {username} - {action}: {detail}";

            // Using StreamWriter with FileStream for async I/O
            using var fileStream = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, useAsync: true);
            using var writer = new StreamWriter(fileStream, Encoding.UTF8);
            await writer.WriteLineAsync(logEntry);
            await writer.FlushAsync();
        }
    }
}
