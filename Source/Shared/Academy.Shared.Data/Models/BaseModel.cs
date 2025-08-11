using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models
{
    public abstract class BaseModel
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string CreatedBy { get; set; } = "System";
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string UpdatedBy { get; set; } = "System";
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsEnabled { get; set; } = true;
    }
}
