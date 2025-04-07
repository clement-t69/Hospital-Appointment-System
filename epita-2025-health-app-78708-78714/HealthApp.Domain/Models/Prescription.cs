using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace HealthApp.Domain.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        [ForeignKey("DoctorId")]
        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("PatientId")]
        public string PatientId { get; set; }
        public Patient Patient { get; set; }

        public string Name { get; set; }
        public string Date { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Pharmacy { get; set; }
    }
}
