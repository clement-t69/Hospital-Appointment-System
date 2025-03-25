using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using HealthApp.Domain.Data;

namespace HealthApp.Factories
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=../HealthApp.MVC/HealthApp.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}