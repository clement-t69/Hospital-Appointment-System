using System.ComponentModel.DataAnnotations;
using HealthApp.Domain.Models;

namespace HealthApp.MVC.Models
{
    public class EditAppointmentInputModel
    {
        public int Id { get; set; }

        //[Required]
        public string Date { get; set; }

        //[Required]
        public string Hour { get; set; }

        //[Required]
        public string DId { get; set; }

        //[Required]
        //public string DoctorLastName { get; set; }

        //[Required]
        //public string DoctorFirstName { get; set; }

        //[Required]
        public string PId { get; set; }

        //[Required]
        public string PLastName { get; set; }

        //[Required]
        public string PFirstName { get; set; }

        //[Required]
        public string Spe { get; set; }

        //[Required]
        public string Loc { get; set; }

        //[Required]
        public string Stat { get; set; }
    }
}