
using Academy.Shared.Security.Models;

namespace Academy.Shared.Security
{
    public interface IAuthClient
    {
        string ProviderName { get; }

        Task<UserProfile?> CreateUserAsync(string firstName, string lastName, string email);
        Task<UserProfile?> GetUserByEmailAsync(string email);
    }
}
