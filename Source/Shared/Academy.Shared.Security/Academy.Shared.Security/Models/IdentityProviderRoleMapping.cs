namespace Academy.Shared.Security.Models
{
    public record IdentityProviderRoleMapping(
        string Issuer,
        string ExternalClaimValue,
        string AppRole
    );
}
