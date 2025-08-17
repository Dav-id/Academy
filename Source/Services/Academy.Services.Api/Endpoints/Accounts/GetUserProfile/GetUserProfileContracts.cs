namespace Academy.Services.Api.Endpoints.Accounts.GetUserProfile
{
    public static class GetUserProfileContracts
    {
        //public record Request(string Id);

        public record GetUserProfileResponse(long Id,
                               string FirstName,
                               string LastName,
                               string Email,
                               bool IsEnabled);
    }
}
