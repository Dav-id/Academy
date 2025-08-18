using System.ComponentModel.DataAnnotations;

namespace Academy.Shared.Data.Models.Roles
{
    public class ExternalRoleMapping : BaseModelNoTenant
    {
        // e.g. "https://login.microsoftonline.com/{tenantId}/v2.0" or "https://{your-domain}.auth0.com/"
        [Required]
        public string   Issuer              { get; set; } = default!;

        // The external claim type to match: "roles", "groups", "permissions", or a custom URI
        [Required]
        public string   ExternalClaimType   { get; set; } = default!;

        // The exact value we expect from the IdP: e.g., "Finance.Admin" or an Azure AD group GUID
        [Required]
        public string   ExternalClaimValue  { get; set; } = default!;

        // Your app role to grant when this claim is present: e.g., "Admin", "Instructor"
        [Required]
        public string   AppRole             { get; set; } = default!;
    }
}
