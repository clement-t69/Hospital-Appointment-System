using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class AppointmentInputModel
    {
        //[Required]
        public string appointmentDate { get; set; }

        //[Required]
        public string appointmentHour { get; set; }

        //[Required]
        public string DoctorId { get; set; }

        //[Required]
        public string DoctorLastName { get; set; }

        //[Required]
        public string DoctorFirstName { get; set; }

        //[Required]
        public string PatientId { get; set; }

        //[Required]
        public string PatientLastName { get; set; }

        //[Required]
        public string PatientFirstName { get; set; }

        //[Required]
        public string Specialization { get; set; }

        //[Required]
        public string Location { get; set; }

        //[Required]
        public string Status { get; set; }
    }
}