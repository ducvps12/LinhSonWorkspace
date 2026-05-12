using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinhSonWorkspace.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
