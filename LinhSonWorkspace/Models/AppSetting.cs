using System.ComponentModel.DataAnnotations;

namespace LinhSonWorkspace.Models
{
    /// <summary>
    /// Stores application configuration as key-value pairs.
    /// Used for SMTP settings, general settings, etc.
    /// </summary>
    public class AppSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [MaxLength(500)]
        public string SettingValue { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = "General";

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
