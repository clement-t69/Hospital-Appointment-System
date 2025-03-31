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

        public List<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}