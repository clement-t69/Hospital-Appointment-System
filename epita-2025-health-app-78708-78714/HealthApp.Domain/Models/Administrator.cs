namespace HealthApp.Domain.Models
{
    public class Administrator: User
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
