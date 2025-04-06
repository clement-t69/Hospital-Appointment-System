using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.Domain.Models
{
    public class Patient
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public User User { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public List<Message> Notifications { get; set; } = new List<Message>();
    }
}