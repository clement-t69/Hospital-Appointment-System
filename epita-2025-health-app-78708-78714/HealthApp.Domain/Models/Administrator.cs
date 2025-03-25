using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.Domain.Models
{
    public class Administrator
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }

        //public IdentityUser User { get; set; }
    }
}