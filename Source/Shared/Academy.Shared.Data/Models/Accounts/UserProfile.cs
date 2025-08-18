using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Accounts
{
    public class UserProfile : BaseModel
    {
        [Required]
        [MaxLength(100)]
        public string FirstName             { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName              { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email                 { get; set; } = string.Empty;

        // For now we will allow only one provider per account, 
        // later we can refactor to allow multiple if required.
        [Required]
        public string IdentityProvider      { get; set; } = string.Empty;

        [Required]
        public string IdentityProviderId    { get; set; } = string.Empty;
    }
}
