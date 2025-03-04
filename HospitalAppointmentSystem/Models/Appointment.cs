using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalAppointmentSystem.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Status { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
    }
}
