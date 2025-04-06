using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Diagnosis { get; set; }
        
        [ForeignKey("DoctorId")]
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }

        [ForeignKey("PatientId")]
        public string PatientId { get; set; }
        public Patient Patient { get; set; }
        public string PatientLastName { get; set; }

        public string Specialization { get; set; }
        public string Location { get; set; }
    }
}
