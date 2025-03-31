using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class ForgotPasswordInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }
    }
}