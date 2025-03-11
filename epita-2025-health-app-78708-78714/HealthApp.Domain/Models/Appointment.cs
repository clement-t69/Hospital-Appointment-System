using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        [ForeignKey("DoctorId")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("PatientId")]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

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
