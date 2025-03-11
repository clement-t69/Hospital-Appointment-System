namespace HealthApp.Domain.Models
{
    public class Patient : User
    {
        public string Id { get; set; }
        public List<MedicalHistory> MedicalHistories { get; set; }
        public List<Prescription> Prescriptions { get; set; }
    }
}
