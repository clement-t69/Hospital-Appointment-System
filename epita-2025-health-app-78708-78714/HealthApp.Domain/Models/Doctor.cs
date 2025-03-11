namespace HealthApp.Domain.Models
{
    public class Doctor: User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public string Location { get; set; }
    }
}
