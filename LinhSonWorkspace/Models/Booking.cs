using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinhSonWorkspace.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingCode { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public int WorkspaceId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, CheckedIn, Completed, Cancelled

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(500)]
        public string Note { get; set; } = string.Empty;

        // Optimistic concurrency
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("WorkspaceId")]
        public virtual Workspace? Workspace { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }
    }
}
