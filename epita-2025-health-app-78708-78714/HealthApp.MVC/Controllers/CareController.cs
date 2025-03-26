using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthApp.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthApp.MVC.Controllers
{
    public class CareController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public CareController(SignInManager<User> signInManager,
            ILogger<AccountController> logger, ApplicationDbContext context, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        // ERROR
        public IActionResult error()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            _logger.LogError($"******************************\nUser {user.Email} has encountered an error. (redirected to error page)\n******************************\n");
            return View();
        }

        public IActionResult patients()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> create_appointment (DateTime date, TimeSpan time, string doctorId, string patientId)
        {
            var appointment = new Appointment
            {
                Date = date,
                Time = time,
                DoctorId = doctorId,
                PatientId = patientId
            };

            _context.Appointments.Add(appointment);

            var doctor = await _userManager.FindByIdAsync(doctorId);
            var patient = await _userManager.FindByIdAsync(patientId);

            _logger.LogInformation($"******************************\nAppointment created for patient {patient.Email} with doctor {doctor.Email} on {date} at {time}.\n******************************\n");
            await _context.SaveChangesAsync();
            return RedirectToAction("appointments", "care");
        }

        public async Task<IActionResult> appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = await _userManager.IsInRoleAsync(user, "doctor");
            ViewBag.IsPatient = await _userManager.IsInRoleAsync(user, "patient");
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "administrator");

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == user.Id.ToString())
                .ToListAsync();

            var sortedAppointments = appointments
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();

            ViewBag.Appointments = sortedAppointments;

            return View();
        }
    }
}