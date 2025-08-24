using Academy.Shared.Security;
using System.Collections.Concurrent;

namespace Academy.Tests.Fakes
{
    public class FakeAuthClient : IAuthClient
    {
        // In-memory stores
        public ConcurrentDictionary<string, Shared.Security.Models.UserProfile> UsersByEmail { get; } = new();
        public ConcurrentDictionary<string, Shared.Security.Models.UserProfile> UsersById { get; } = new();
        public HashSet<string> CreatedRoles { get; } = new();
        public List<(string UserId, string Role)> UserRoleAssignments { get; } = new();

        public string ProviderName { get; set; } = "FakeProvider";

        // For test assertions
        public List<(string FirstName, string LastName, string Email)> CreatedUsers { get; } = new();

        public Task<Shared.Security.Models.UserProfile?> CreateUserAsync(string firstName, string lastName, string email)
        {
            // If user already exists, return it
            if (UsersByEmail.TryGetValue(email, out Shared.Security.Models.UserProfile? existing))
            {
                return Task.FromResult<Shared.Security.Models.UserProfile?>(existing);
            }

            // Simulate user creation
            string id = Guid.NewGuid().ToString();
            Shared.Security.Models.UserProfile user = new(id, firstName, lastName, email, true, []);
            UsersByEmail[email] = user;
            UsersById[id] = user;
            CreatedUsers.Add((firstName, lastName, email));
            return Task.FromResult<Shared.Security.Models.UserProfile?>(user);
        }

        public Task<Shared.Security.Models.UserProfile?> GetUserByEmailAsync(string email)
        {
            UsersByEmail.TryGetValue(email, out Shared.Security.Models.UserProfile? user);
            return Task.FromResult<Shared.Security.Models.UserProfile?>(user);
        }

        public Task CreateRoleAsync(string role)
        {
            CreatedRoles.Add(role);
            return Task.CompletedTask;
        }

        public Task AddUserToRoleAsync(string id, string role)
        {
            // Avoid duplicate assignments
            if (!UserRoleAssignments.Contains((id, role)))
            {
                UserRoleAssignments.Add((id, role));
            }
            return Task.CompletedTask;
        }

        // Optional: helper to seed users for tests
        public void SeedUser(Shared.Security.Models.UserProfile user)
        {
            UsersByEmail[user.Email] = user;
            UsersById[user.Id] = user;
        }

        // Optional: helper to clear all state
        public void Clear()
        {
            UsersByEmail.Clear();
            UsersById.Clear();
            CreatedRoles.Clear();
            UserRoleAssignments.Clear();
            CreatedUsers.Clear();
        }
    }
}
