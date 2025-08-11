using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Academy.Shared.Data.Contexts
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("###", options =>
            {
                options.MigrationsAssembly("Academy.Shared.Data");
                options.EnableRetryOnFailure();
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
