namespace Academy.Services.Api.Endpoints.Account.GetUserProfile
{
    public static class Contracts
    {
        //public record Request(string Id);

        public record Response(string Id,
                               string FirstName,
                               string LastName,
                               string Email,
                               bool IsEnabled);
    }
}
