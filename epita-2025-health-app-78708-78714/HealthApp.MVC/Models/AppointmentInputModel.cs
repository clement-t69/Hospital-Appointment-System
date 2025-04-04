using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class AppointmentInputModel
    {
        public BookAppointmentInputModel bookAppointment { get; set; }
        public EditAppointmentInputModel editAppointment { get; set; }
    }
}