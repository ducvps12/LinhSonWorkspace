using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LinhSonWorkspace.Models;

namespace LinhSonWorkspace.Services
{
    /// <summary>
    /// Handles export functionality for bookings data.
    /// Covers Stream I/O requirement for PRN212.
    /// </summary>
    public class ExportService
    {
        /// <summary>
        /// Exports a list of bookings to CSV format using StreamWriter.
        /// Demonstrates Stream I/O operations.
        /// </summary>
        public async Task ExportBookingsToCsvAsync(IEnumerable<Booking> bookings, string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            using var writer = new StreamWriter(fileStream, Encoding.UTF8);

            // Write CSV header
            await writer.WriteLineAsync("BookingCode,Customer,Workspace,StartTime,EndTime,TotalPrice,Status,CreatedBy,CreatedAt,Note");

            // Write each booking as a CSV row
            foreach (var booking in bookings)
            {
                var line = string.Join(",",
                    EscapeCsvField(booking.BookingCode),
                    EscapeCsvField(booking.Customer?.FullName ?? ""),
                    EscapeCsvField(booking.Workspace?.Name ?? ""),
                    booking.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    booking.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    booking.TotalPrice.ToString("N0"),
                    booking.Status,
                    EscapeCsvField(booking.CreatedByUser?.FullName ?? ""),
                    booking.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    EscapeCsvField(booking.Note));

                await writer.WriteLineAsync(line);
            }

            await writer.FlushAsync();
        }

        /// <summary>
        /// Exports a list of bookings to JSON format.
        /// </summary>
        public async Task ExportBookingsToJsonAsync(IEnumerable<Booking> bookings, string filePath)
        {
            var exportData = bookings.Select(b => new
            {
                b.BookingCode,
                Customer = b.Customer?.FullName ?? "",
                Workspace = b.Workspace?.Name ?? "",
                StartTime = b.StartTime.ToString("yyyy-MM-dd HH:mm"),
                EndTime = b.EndTime.ToString("yyyy-MM-dd HH:mm"),
                TotalPrice = b.TotalPrice.ToString("N0"),
                b.Status,
                CreatedBy = b.CreatedByUser?.FullName ?? "",
                CreatedAt = b.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                b.Note
            }).ToList();

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(exportData, options);

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            using var writer = new StreamWriter(fileStream, Encoding.UTF8);
            await writer.WriteAsync(json);
            await writer.FlushAsync();
        }

        /// <summary>
        /// Escapes a field for CSV format (handles commas and quotes).
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}
