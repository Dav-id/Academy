namespace Academy.Shared.Security.Models
{
    public record UserProfile(
        string Id = "",
        string FirstName = "",
        string LastName = "",
        string Email = "",
        bool IsEnabled = true,
        string[]? Roles = null
    )
    {
        public string[] Roles { get; init; } = Roles ?? [];
    }
}
