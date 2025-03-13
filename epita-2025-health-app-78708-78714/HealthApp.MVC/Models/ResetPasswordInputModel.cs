using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class ResetPasswordInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }
    }
}