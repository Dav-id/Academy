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

        public DbSet<ExternalRoleMapping> ExternalRoleMappings { get; set; }
    }
}
