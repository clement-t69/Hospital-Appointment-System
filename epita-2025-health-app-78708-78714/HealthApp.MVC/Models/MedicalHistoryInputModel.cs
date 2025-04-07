using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class MedicalHistoryInputModel
    {
        public string Diagnosis { get; set; }
    }
}