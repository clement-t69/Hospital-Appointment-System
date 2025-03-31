using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        public Doctor Doctor { get; set; }
        public string DoctorId { get; set; }
        public string DoctorLastName { get; set; }

        public Patient Patient { get; set; }
        public string PatientId { get; set; }
        public string PatientLastName { get; set; }

        public string Status { get; set; }
        /*
            Pending,
            Approved,
            Rejected,
            Completed,
            Cancelled
        */
    }
}
