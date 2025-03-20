using System.ComponentModel.DataAnnotations.Schema;

namespace HealthApp.Domain.Models
{
    public class Administrator
    {
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }
    }
}
