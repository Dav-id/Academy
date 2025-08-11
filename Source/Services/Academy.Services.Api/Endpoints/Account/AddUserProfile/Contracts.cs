namespace Academy.Services.Api.Endpoints.Account.AddUserProfile
{
    public static class Contracts
    {
        public record Request(string FirstName,
                              string LastName,
                              string Email);

        public record Response(string Id,
                               string FirstName,
                               string LastName,
                               string Email,
                               bool IsEnabled);
    }
}
