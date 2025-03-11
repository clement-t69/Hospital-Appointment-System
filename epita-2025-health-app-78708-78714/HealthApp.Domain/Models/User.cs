using Microsoft.AspNetCore.Identity;

namespace HealthApp.Domain.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
