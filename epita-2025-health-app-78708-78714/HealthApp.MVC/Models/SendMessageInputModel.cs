using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class SendMessageInputModel
    {
        public NewMessageInputModel NewMessage { get; set; }
        public ReplyMessageInputModel ReplyMessage { get; set; }
    }
}