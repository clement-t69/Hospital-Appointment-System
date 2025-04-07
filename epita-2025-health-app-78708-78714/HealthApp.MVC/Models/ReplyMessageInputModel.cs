using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class ReplyMessageInputModel
    {
        public int Id { get; set; }

        [Required]
        public string replySenderId { get; set; }

        [Required]
        public string replyReceiverId { get; set; }

        [Required]
        public string replyObject { get; set; }

        [Required]
        public string replyMessage { get; set; }

        [Required]
        public string replyDate { get; set; }

        [Required]
        public string replyType { get; set; }
        /*
         New
         Reply
         */
    }
}