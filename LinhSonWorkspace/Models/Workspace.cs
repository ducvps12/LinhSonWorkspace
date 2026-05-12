using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinhSonWorkspace.Models
{
    public class Workspace
    {
        [Key]
        public int WorkspaceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int TypeId { get; set; }

        public int Capacity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerHour { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Available"; // Available, Maintenance, Inactive

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation
        [ForeignKey("TypeId")]
        public virtual WorkspaceType? WorkspaceType { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
