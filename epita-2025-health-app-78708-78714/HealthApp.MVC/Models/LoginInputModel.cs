using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class LoginInputModel
    {
        [Required]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [Display(Name = "Stay connected")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}