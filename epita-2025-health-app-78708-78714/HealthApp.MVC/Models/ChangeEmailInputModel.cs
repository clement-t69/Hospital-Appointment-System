using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class ChangeEmailInputModel
    {
        [Required(ErrorMessage = "The Current Email is required.")]
        [EmailAddress]
        [Display(Name = "Current Email")]
        public string? CurrentEmail { get; set; }

        [Required(ErrorMessage = "The New Email is required.")]
        [EmailAddress]
        [Display(Name = "New Email")]
        public string? NewEmail { get; set; }

        [Required(ErrorMessage = "The Confirm New Email is required.")]
        [EmailAddress]
        [Display(Name = "Confirm New Email")]
        [Compare("NewEmail", ErrorMessage = "The Emails needs to match.")]
        public string? ConfirmNewEmail { get; set; }

        public ChangeEmailInputModel(string currentEmail, string newEmail, string confirmNewEmail)
        {
            CurrentEmail = currentEmail;
            NewEmail = newEmail;
            ConfirmNewEmail = confirmNewEmail;
        }

        public ChangeEmailInputModel() { }
    }
}