using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class RegisterInputModel
    {
        [Required]
        [Display(Name = "First Name")]
        public required string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public required string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public required string Phone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The passwords needs to match.")]
        public required string ConfirmPassword { get; set; }

        [Display(Name = "Address")]
        public required string Address { get; set; }

        public string? ReturnUrl { get; set; }
    }
}