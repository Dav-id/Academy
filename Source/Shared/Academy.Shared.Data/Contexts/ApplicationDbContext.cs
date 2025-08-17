using Academy.Shared.Data.Models;
using Academy.Shared.Data.Models.Accounts;
using Academy.Shared.Data.Models.Courses;
using Academy.Shared.Data.Models.Roles;
using Academy.Shared.Data.Models.Tenants;

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

        // This property is used to filter data based on the tenant ID.
        /// <summary>
        /// The ID of the tenant for which the current context is being used.
        /// </summary>
        public long TenantId { get; private set; }

        /// <summary>
        /// Sets the tenant ID for the current context.
        /// </summary>
        /// <param name="tenantId"></param>
        public void SetTenant(long tenantId)
        {
            TenantId = tenantId;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Accounts
            modelBuilder.Entity<UserProfile>().HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            
            //Courses
            modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            modelBuilder.Entity<CourseModule>().HasQueryFilter(c => !c.IsDeleted && c.TenantId == TenantId);
            
            //Roles
            modelBuilder.Entity<ExternalRoleMapping>().HasQueryFilter(c => !c.IsDeleted/* && c.TenantId == TenantId*/);

            //Tenants
            modelBuilder.Entity<Tenant>().HasQueryFilter(c => !c.IsDeleted);
        }

        // Accounts
        public DbSet<UserProfile>           UserProfiles            { get; set; }

        // Courses
        public DbSet<Course>                Courses                 { get; set; }

        // Roles
        public DbSet<ExternalRoleMapping>   ExternalRoleMappings    { get; set; }

        // Tenants
        public DbSet<Tenant>                Tenants                 { get; set; }

    }
}
