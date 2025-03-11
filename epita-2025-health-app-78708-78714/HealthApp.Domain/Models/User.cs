using Microsoft.AspNet.Identity.EntityFramework;

namespace HealthApp.Domain.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
