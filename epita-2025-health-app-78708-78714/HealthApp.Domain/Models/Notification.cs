using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [ForeignKey("DoctorId")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("PatientId")]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}
