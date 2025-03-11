using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HospitalAppointmentSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointmentSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patient
        public async Task<IActionResult> Index()
        {
            var patientId = _userManager.GetUserId(User);
            var patient = await _context.Patients.FirstOrDefaultAsync(m => m.Id.ToString() == patientId);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patient/Create
        public IActionResult Create()
        {
            return View();
        }
    }
}
