using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Patient : User
    {
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public int UserId { get; set; }

        public List<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public List<Appointment> Appointments { get; set; } = new List<Appointment>();

        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
