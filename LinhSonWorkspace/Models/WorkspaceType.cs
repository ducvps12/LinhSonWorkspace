using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinhSonWorkspace.Models
{
    public class WorkspaceType
    {
        [Key]
        public int TypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation
        public virtual ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
    }
}
