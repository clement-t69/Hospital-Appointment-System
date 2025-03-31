using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class MedicalHistoryInputModel
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Diagnosis { get; set; }
        [Required]
        public string DoctorId { get; set; }
        [Required]
        public string PatientId { get; set; }
        [Required]
        public string Specialization { get; set; }
        [Required]
        public string Location { get; set; }
    }
}