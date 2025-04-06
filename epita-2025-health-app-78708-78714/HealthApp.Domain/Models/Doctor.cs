using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.Domain.Models
{
    public class Doctor
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public User User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Specialization { get; set; }
        public string Location { get; set; }
        public List<Message> Notifications { get; set; } = new List<Message>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}