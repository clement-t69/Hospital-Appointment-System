using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class EditMedicalHistoryInputModel
    {
        public string Date { get; set; }

        public string Diagnosis { get; set; }

        public string DoctorId { get; set; }

        public string PatientId { get; set; }

        public string Specialization { get; set; }

        public string Location { get; set; }
    }
}