namespace HealthApp.Domain.Models
{
    public class Patient : User
    {
        public string Email { get; set; }
        public List<MedicalHistory> MedicalHistories { get; set; }
        public List<Prescription> Prescriptions { get; set; }
    }
}
