namespace HealthApp.Domain.Models
{
    public class Prescription
    {
        public string Id { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Name { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public string Pharmacy { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
