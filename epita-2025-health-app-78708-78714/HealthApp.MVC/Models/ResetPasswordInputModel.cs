using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class ResetPasswordInputModel
    {
        [Required(ErrorMessage = "The New Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "The Confirm New Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New password")]
        [Compare("NewPassword", ErrorMessage = "The Passwords needs to match.")]
        public string? ConfirmNewPassword { get; set; }

        public string Token { get; set; }
        public string Email { get; set; }

        public ResetPasswordInputModel(string currentPassword, string newPassword, string confirmNewPassword, string token, string email)
        {
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
            Token = token;
            Email = email;
        }

        public ResetPasswordInputModel() { }
    }
}