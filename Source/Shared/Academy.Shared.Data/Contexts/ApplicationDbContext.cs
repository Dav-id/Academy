using Academy.Shared.Data.Models.Accounts;
using Academy.Shared.Data.Models.Roles;

using Microsoft.EntityFrameworkCore;

namespace Academy.Shared.Data.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected ApplicationDbContext()
        {
        }

        // Accounts
        public DbSet<UserProfile>           UserProfiles            { get; set; }

        // Roles
        public DbSet<ExternalRoleMapping>   ExternalRoleMappings    { get; set; }
    }
}
