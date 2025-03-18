using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "The Current Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "The New Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "The Confirm New Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New password")]
        [Compare("NewPassword", ErrorMessage = "The Passwords needs to match.")]
        public string? ConfirmNewPassword { get; set; }

        public ChangePasswordInputModel(string currentPassword, string newPassword, string confirmNewPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }

        public ChangePasswordInputModel() { }
    }
}