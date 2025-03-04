using Microsoft.AspNetCore.Identity;

namespace HospitalAppointmentSystem.Models

{
    public class Users : IdentityUser
    {
        public string Name { get; set; }
    }
}
