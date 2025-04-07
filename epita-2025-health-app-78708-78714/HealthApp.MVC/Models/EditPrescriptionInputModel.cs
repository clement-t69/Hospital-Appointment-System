using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class EditPrescriptionInputModel
    {
        public string Date { get; set; }

        public string DoctorId { get; set; }

        public string PatientId { get; set; }

        public string Name { get; set; }

        public string Dosage { get; set; }

        public string Frequency { get; set; }

        public string Duration { get; set; }

        public string Pharmacy { get; set; }
    }
}