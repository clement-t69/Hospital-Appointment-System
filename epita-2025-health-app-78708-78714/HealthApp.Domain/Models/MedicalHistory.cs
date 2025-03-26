using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }

        [ForeignKey("DoctorId")]
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("PatientId")]
        public string PatientId { get; set; }
        public Patient Patient { get; set; }
    }
}
