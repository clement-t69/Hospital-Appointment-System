using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string SenderId { get; set; }
        public string SenderLastName { get; set; }
        public string SenderFirstName { get; set; }

        public string ReceiverId { get; set; }
        public string ReceiverLastName { get; set; }
        public string ReceiverFirstName { get; set; }

        public Doctor Doctor { get; set; }
        [ForeignKey("DoctorId")]
        public string DoctorId { get; set; }

        public Patient Patient { get; set; }
        [ForeignKey("PatientId")]
        public string PatientId { get; set; }

        public string Object { get; set; }
        public string Content { get; set; }

        public string Date { get; set; }

        public string Type { get; set; }
        /*
        New
         Reply
         */

        public bool IsRead { get; set; } = false;
    }
}
