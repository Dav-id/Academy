using Academy.Shared.Localisation;

using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Accounts
{
    public class UserProfile : BaseModelNoId
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        [MinLength(1, ErrorMessageResourceName = "Global__Field__MinLength", ErrorMessageResourceType = typeof(ModelTranslation))]
        [MaxLength(100, ErrorMessageResourceName = "Global__Field__MaxLength", ErrorMessageResourceType = typeof(ModelTranslation))]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        [MinLength(1, ErrorMessageResourceName = "Global__Field__MinLength", ErrorMessageResourceType = typeof(ModelTranslation))]
        [MaxLength(100, ErrorMessageResourceName = "Global__Field__MaxLength", ErrorMessageResourceType = typeof(ModelTranslation))]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "Global__Field__Required", ErrorMessageResourceType = typeof(ModelTranslation))]
        [MinLength(5, ErrorMessageResourceName = "Global__Field__MinLength", ErrorMessageResourceType = typeof(ModelTranslation))]
        [EmailAddress(ErrorMessageResourceName = "Accounts__UserProfile__InvalidEmail", ErrorMessageResourceType = typeof(ModelTranslation))]
        [MaxLength(100, ErrorMessageResourceName = "Global__Field__MaxLength", ErrorMessageResourceType = typeof(ModelTranslation))]
        public string Email { get; set; } = string.Empty;
    }
}
