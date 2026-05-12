using System;
using System.Linq;
using System.Threading.Tasks;
using LinhSonWorkspace.Data;
using LinhSonWorkspace.Models;
using Microsoft.EntityFrameworkCore;

namespace LinhSonWorkspace.Services
{
    /// <summary>
    /// Core business logic for booking management.
    /// Includes conflict detection and concurrency handling.
    /// </summary>
    public class BookingService
    {
        /// <summary>
        /// Checks if a workspace is already booked during the specified time range.
        /// Business rule: Two time ranges conflict if startTime < existingEnd AND endTime > existingStart.
        /// </summary>
        public async Task<bool> HasConflictAsync(int workspaceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null)
        {
            using var context = new AppDbContext();

            var query = context.Bookings
                .Where(b => b.WorkspaceId == workspaceId
                    && b.Status != "Cancelled"
                    && startTime < b.EndTime
                    && endTime > b.StartTime);

            // Exclude current booking when editing
            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.BookingId != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Generates the next booking code in format BKxxx.
        /// </summary>
        public async Task<string> GenerateBookingCodeAsync()
        {
            using var context = new AppDbContext();

            var lastBooking = await context.Bookings
                .OrderByDescending(b => b.BookingId)
                .FirstOrDefaultAsync();

            if (lastBooking == null) return "BK001";

            // Extract number from booking code
            var lastCode = lastBooking.BookingCode;
            if (lastCode.StartsWith("BK") && int.TryParse(lastCode.Substring(2), out int num))
            {
                return $"BK{(num + 1):D3}";
            }

            return $"BK{(lastBooking.BookingId + 1):D3}";
        }

        /// <summary>
        /// Calculates total price based on workspace pricing and duration.
        /// Uses per-hour pricing if duration < 8 hours, per-day pricing otherwise.
        /// </summary>
        public decimal CalculatePrice(Workspace workspace, DateTime startTime, DateTime endTime)
        {
            var duration = endTime - startTime;
            var hours = (decimal)duration.TotalHours;

            if (hours >= 8)
            {
                // Use per-day pricing
                var days = Math.Ceiling((double)hours / 24);
                return workspace.PricePerDay * (decimal)days;
            }
            else
            {
                // Use per-hour pricing, round up to nearest hour
                var roundedHours = Math.Ceiling((double)hours);
                return workspace.PricePerHour * (decimal)roundedHours;
            }
        }

        /// <summary>
        /// Creates a new booking with conflict detection.
        /// Throws InvalidOperationException if time conflict is detected.
        /// </summary>
        public async Task<Booking> CreateBookingAsync(int customerId, int workspaceId,
            DateTime startTime, DateTime endTime, string note, int createdByUserId)
        {
            // Check for time conflicts
            bool hasConflict = await HasConflictAsync(workspaceId, startTime, endTime);
            if (hasConflict)
            {
                throw new InvalidOperationException(
                    "This workspace is already booked in the selected time range. Please choose a different time or workspace.");
            }

            using var context = new AppDbContext();

            var workspace = await context.Workspaces.FindAsync(workspaceId)
                ?? throw new InvalidOperationException("Workspace not found.");

            var booking = new Booking
            {
                BookingCode = await GenerateBookingCodeAsync(),
                CustomerId = customerId,
                WorkspaceId = workspaceId,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = CalculatePrice(workspace, startTime, endTime),
                Status = "Pending",
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.Now,
                Note = note ?? string.Empty
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            return booking;
        }

        /// <summary>
        /// Updates booking status with optimistic concurrency control.
        /// Handles DbUpdateConcurrencyException when two users modify the same booking.
        /// </summary>
        public async Task UpdateBookingStatusAsync(int bookingId, string newStatus)
        {
            using var context = new AppDbContext();

            var booking = await context.Bookings.FindAsync(bookingId)
                ?? throw new InvalidOperationException("Booking not found.");

            booking.Status = newStatus;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException(
                    "This booking has been modified by another user. Please refresh and try again.");
            }
        }
    }
}
