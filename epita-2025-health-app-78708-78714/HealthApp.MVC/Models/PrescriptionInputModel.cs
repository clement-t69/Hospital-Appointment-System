using System.ComponentModel.DataAnnotations;

namespace HealthApp.MVC.Models
{
    public class PrescriptionInputModel
    {
        [Required]
        public string DoctorId { get; set; }
        [Required]
        public string PatientId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Dosage { get; set; }
        [Required]
        public int Frequency { get; set; }
        [Required]
        public int Duration { get; set; }
        [Required]
        public string Pharmacy { get; set; }
        [Required]
        public string Date { get; set; }
    }
}