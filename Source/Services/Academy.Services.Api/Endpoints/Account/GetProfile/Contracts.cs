namespace Academy.Services.Api.Endpoints.Account.GetProfile
{
    public static class Contracts
    {
        //public record Request(string Id);

        public record Response(string Id,
                               string FirstName,
                               string LastName,
                               bool IsEnabled);
    }
}
