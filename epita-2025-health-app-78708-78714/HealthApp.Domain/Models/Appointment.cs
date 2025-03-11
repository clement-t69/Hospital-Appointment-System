using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthApp.Domain.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
    }
}
