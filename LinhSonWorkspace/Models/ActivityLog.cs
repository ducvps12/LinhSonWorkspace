using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinhSonWorkspace.Models
{
    public class ActivityLog
    {
        [Key]
        public int LogId { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Detail { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
