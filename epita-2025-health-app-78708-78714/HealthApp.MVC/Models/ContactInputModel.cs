using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class ContactInputModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string Object { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string Date { get; set; }
    }
}