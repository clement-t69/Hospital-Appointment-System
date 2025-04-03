using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [ForeignKey("DoctorId")]
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("PatientId")]
        public string PatientId { get; set; }
        public Patient Patient { get; set; }

        public string Object { get; set; }
        public string Message { get; set; }

        public string Date { get; set; }
    }
}
