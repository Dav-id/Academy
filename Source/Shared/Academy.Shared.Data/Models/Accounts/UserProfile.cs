using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Accounts
{
    public class UserProfile : BaseModelNoId
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}
