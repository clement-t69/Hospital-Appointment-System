using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.MVC.Data
{
    public class MVCDbContext : IdentityDbContext
    {
        public MVCDbContext(DbContextOptions<MVCDbContext> options)
            : base(options)
        {
        }
    }
}
