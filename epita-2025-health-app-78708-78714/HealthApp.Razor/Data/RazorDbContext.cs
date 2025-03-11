using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.Razor.Data
{
    public class RazorDbContext : IdentityDbContext
    {
        public RazorDbContext(DbContextOptions<RazorDbContext> options)
            : base(options)
        {
        }
    }
}
