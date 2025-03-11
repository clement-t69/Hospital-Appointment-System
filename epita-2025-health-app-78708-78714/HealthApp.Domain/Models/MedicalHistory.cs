namespace HealthApp.Domain.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string DoctorName { get; set; }
    }
}
