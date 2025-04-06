namespace HealthApp.Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }

        public string SenderId { get; set; }
        public string ReceiverId { get; set; }

        public bool IsRead { get; set; } = false;
    }
}
