using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.Domain.Data
{
    public class DbSeeder
    {
        public static async Task Seed(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Doctor"))
                await roleManager.CreateAsync(new IdentityRole("Doctor"));
            if (!await roleManager.RoleExistsAsync("Patient"))
                await roleManager.CreateAsync(new IdentityRole("Patient"));
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new User
                {
                    Name = "Admin",
                    Role = "Admin"
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
