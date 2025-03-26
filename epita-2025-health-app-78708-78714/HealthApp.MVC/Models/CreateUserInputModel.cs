using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class CreateUserInputModel
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

        [Required]
        [Display(Name = "Address")]
        public required string Address { get; set; }

        [Required]
        [Display(Name = "Role")]
        public Role Role { get; set; }
    }
}