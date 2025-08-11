using Academy.Shared.Localisation;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models
{
    public abstract class BaseModelNoId
    {
        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        public string CreatedBy { get; set; } = "System";
        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        public string UpdatedBy { get; set; } = "System";
        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        public bool IsEnabled { get; set; } = true;
    }

    public abstract class BaseModel : BaseModelNoId
    {
        [Key]
        public long Id { get; set; }
    }
}
