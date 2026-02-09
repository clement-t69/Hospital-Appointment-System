using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class ChangeInfoInputModel
    {
        [Required(ErrorMessage = "The Phone Number is required.")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "The Address is required.")]

        [Display(Name = "Address")]
        public string? Address { get; set; }

        public string Location { get; set; }
        public string Specialization { get; set; }

        public ChangeInfoInputModel(string phone, string address, string location, string specialization)
        {
            Phone = phone;
            Address = address;
            Location = location;
            Specialization = specialization;
        }

        public ChangeInfoInputModel() { }
    }
}