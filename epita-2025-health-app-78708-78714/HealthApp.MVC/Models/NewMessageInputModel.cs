using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class NewMessageInputModel
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        [Required]
        public string Object { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string Date { get; set; }

        [Required]
        public string Type { get; set; }
        /*
         New
         Reply
         */
    }
}