using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        public string DoctorId { get; set; }

        public string PatientId { get; set; }

        public enum Status
        {
            Pending,
            Approved,
            Rejected,
            Completed,
            Cancelled
        }
    }
}
