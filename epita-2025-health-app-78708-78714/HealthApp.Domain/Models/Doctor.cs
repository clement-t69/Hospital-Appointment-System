using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.Domain.Models
{
    public class Doctor
    {
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }

        //public IdentityUser User { get; set; }

        [ForeignKey("SpecializationId")]
        public int SpecializationId { get; set; }
        public DoctorSpecialization Specialization { get; set; }
        public string Location { get; set; }
        public List<DoctorAvailability> DoctorAvailabilities { get; set; } = new List<DoctorAvailability>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}