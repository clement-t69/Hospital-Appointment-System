#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthApp.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using HealthApp.MVC.Models;
using System.Data.Entity;
using System.Numerics;

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

        // care/patients.cshtml
        public async Task<IActionResult> patients([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            if (user == null)
            {
                return View();
            }
            var userRoles = await _signInManager.UserManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            var patients = await _userManager.GetUsersInRoleAsync("Patient");

            if (!string.IsNullOrEmpty(searchInput) && !string.IsNullOrEmpty(searchField))
            {
                searchInput = searchInput.ToLower();

                if (searchField == "Name")
                {
                    patients = patients.Where(u => u.FirstName.ToLower().Contains(searchInput) || u.LastName.ToLower().Contains(searchInput)).ToList();
                }
                else if (searchField == "Email")
                {
                    patients = patients.Where(u => u.UserName.ToLower().Contains(searchInput)).ToList();
                }
                _logger.LogInformation($"******************************\nAdmin {user.Email} has searched for users with {searchField} containing {searchInput}.\n******************************\n");
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            ViewBag.Patients = patients;

            return View();
        }

        // care/patient.cshtml
        public async Task<IActionResult> patient(string id, 
            [FromQuery] string appointmentsSearchInput, [FromQuery] string appointmentsSearchField,
            [FromQuery] string prescriptionsSearchInput, [FromQuery] string prescriptionsSearchField,
            [FromQuery] string medicalHistoriesSearchInput, [FromQuery] string medicalHistoriesSearchField)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            var patient = await _userManager.FindByIdAsync(id);
            if (patient == null)
            {
                _logger.LogError($"******************************\nUser {user.Email} has attempted to view a patient that does not exist.\n******************************\n");
                TempData["ErrorMessage"] = "Patient does not exist.";
                return RedirectToAction("patients", "care");
            }

            ViewBag.Patient = patient;

            List<Appointment> appointments = await _context.Appointments
                .Where(a => a.PatientId == id)
                .ToListAsync();

            if (appointmentsSearchField != null && appointmentsSearchInput != null)
            {
                appointments = searchAppointment(appointmentsSearchInput, appointmentsSearchField);
            }

            List<Prescription> prescriptions = await _context.Prescriptions
                .Where(p => p.PatientId == id)
                .ToListAsync();

            if (prescriptionsSearchField != null && prescriptionsSearchInput != null)
            {
                prescriptions = searchPrescription(prescriptionsSearchInput, prescriptionsSearchField);
            }

            List<MedicalHistory> medicalHistories = await _context.MedicalHistories
                .Where(mh => mh.PatientId == id)
                .ToListAsync();

            if (medicalHistoriesSearchField != null && medicalHistoriesSearchInput != null)
            {
                medicalHistories = searchMedicalHistory(medicalHistoriesSearchInput, medicalHistoriesSearchField);
            }

            ViewBag.Appointments = appointments;
            ViewBag.Prescriptions = prescriptions;
            ViewBag.MedicalHistories = medicalHistories;

            return View();
        }

        public List<Appointment> searchAppointment(string searchInput, string searchField)
        {
            var appointments = _context.Appointments.ToList();
            if (!string.IsNullOrEmpty(searchInput) && !string.IsNullOrEmpty(searchField))
            {
                searchInput = searchInput.ToLower();
                searchField = searchField.ToLower();
                if (searchField == "Date")
                {
                    appointments = appointments.Where(a => a.Date.ToString().Contains(searchInput)).ToList();
                }
                else if (searchField == "Doctor")
                {
                    appointments = appointments.Where(a => a.DoctorId.Contains(searchInput)).ToList();
                }
                else if (searchField == "Status")
                {
                    appointments = appointments.Where(a => a.Status.ToString().Contains(searchInput)).ToList();
                }
                else
                {
                    searchInput = "";
                    searchField = "";
                }
            }

            return appointments;
        }

        public List<MedicalHistory> searchMedicalHistory(string searchInput, string searchField)
        {
            var medicalHistories = _context.MedicalHistories.ToList();
            if (!string.IsNullOrEmpty(searchInput) && !string.IsNullOrEmpty(searchField))
            {
                searchInput = searchInput.ToLower();
                searchField = searchField.ToLower();
                if (searchField == "Date")
                {
                    medicalHistories = medicalHistories.Where(mh => mh.Date.ToString().Contains(searchInput)).ToList();
                }
                else if (searchField == "Doctor")
                {
                    medicalHistories = medicalHistories.Where(mh => mh.DoctorLastName.Contains(searchInput)).ToList();
                }
                else if (searchField == "Speciality")
                {
                    medicalHistories = medicalHistories.Where(mh => mh.Specialization.Contains(searchInput)).ToList();
                }
                else if (searchField == "Location")
                {
                    medicalHistories = medicalHistories.Where(mh => mh.Location.Contains(searchInput)).ToList();
                }
                else
                {
                    searchInput = "";
                    searchField = "";
                }
            }

            return medicalHistories;
        }

        public List<Prescription> searchPrescription(string searchInput, string searchField)
        {
            var prescriptions = _context.Prescriptions.ToList();
            if (!string.IsNullOrEmpty(searchInput) && !string.IsNullOrEmpty(searchField))
            {
                searchInput = searchInput.ToLower();
                searchField = searchField.ToLower();
                if (searchField == "Date")
                {
                    prescriptions = prescriptions.Where(p => p.Date.ToString().Contains(searchInput)).ToList();
                }
                else if (searchField == "Name")
                {
                    prescriptions = prescriptions.Where(p => p.Name.Contains(searchInput)).ToList();
                }
                else
                {
                    searchInput = "";
                    searchField = "";
                }
            }

            return prescriptions;
        }

        public async Task<IActionResult> create_prescription(string patientId)
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

            var patient = await _userManager.FindByIdAsync(patientId);
            ViewBag.Patient = patient;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> create_prescription(string patientId, PrescriptionInputModel model)
        {
            var patient = await _userManager.FindByIdAsync(patientId);
            if (ModelState.IsValid)
            {
                var prescription = new Prescription
                {
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    Name = model.Name,
                    Dosage = model.Dosage,
                    Frequency = model.Frequency,
                    Duration = model.Duration,
                    Pharmacy = model.Pharmacy,
                    ExpirationDate = model.ExpirationDate,
                    Date = model.Date
                };

                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"******************************\nPrescription created for patient {patient.Email} by doctor {model.DoctorId}.\n******************************\n");
            }

            return RedirectToAction("patient", "care");
        }

        public async Task<IActionResult> create_medical_history(string patientId)
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

            var patient = await _userManager.FindByIdAsync(patientId);
            ViewBag.Patient = patient;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> create_medical_history(string patientId, MedicalHistoryInputModel model)
        {
            var patient = _context.Patients.Find(patientId);
            var p = await _userManager.FindByIdAsync(patientId);
            var d = await _userManager.FindByIdAsync(model.DoctorId);

            if (ModelState.IsValid)
            {
                var medicalHistory = new MedicalHistory
                {
                    Date = model.Date,
                    Diagnosis = model.Diagnosis,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    Specialization = model.Specialization,
                    Location = model.Location
                };

                var doctor = _context.Doctors.Find(model.DoctorId);

                medicalHistory.DoctorLastName = doctor.LastName;
                medicalHistory.PatientLastName = patient.LastName;

                _context.MedicalHistories.Add(medicalHistory);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"******************************\nMedical history created for patient {p.Email} by doctor {model.DoctorId}.\n******************************\n");
            }

            return RedirectToAction("patient", "care");
        }

        // care/doctors.cshtml
        public async Task<IActionResult> doctors([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("login", "account");
            }
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = await _userManager.IsInRoleAsync(user, "doctor");
            ViewBag.IsPatient = await _userManager.IsInRoleAsync(user, "patient");
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "administrator");

            if (searchInput == null || searchField == null)
            {
                searchInput = "";
                searchField = "";
            }

            var doctors = searchDoctors(searchInput, searchField);
            var doctorUsers = new Dictionary<string, User>();

            foreach (var doctor in doctors)
            {
                var u = await _userManager.FindByIdAsync(doctor.UserId);
                if (u != null)
                {
                    doctorUsers[doctor.UserId] = u;
                }
            }

            if (searchInput != "" && searchField != "" && doctors.Count == 0)
            {
                TempData["ErrorMessage"] = "No doctors found.";
                _logger.LogError($"******************************\nNo doctors found while searching for doctors with {searchField} containing {searchInput}.\n******************************\n");
            }

            _logger.BeginScope($"******************************\nUser {user.UserName} has searched for doctors with {searchField} containing {searchInput}.\n******************************\n");

            ViewBag.Doctors = doctors;
            ViewBag.DoctorUsers = doctorUsers;

            return View();
        }

        public List<Doctor> searchDoctors(string searchInput, string searchField)
        {
            var doctors = _context.Doctors.ToList();

            if (searchField == "Name")
            {
                doctors = doctors.Where(d => d.FirstName.ToLower().Contains(searchInput) || d.LastName.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Location")
            {
                doctors = doctors.Where(doctors => doctors.Location.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Specialization")
            {
                doctors = doctors.Where(doctors => doctors.Specialization.ToLower().Contains(searchInput)).ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return doctors;
        }

        // care/appointments.cshtml
        public async Task<IActionResult> appointments(DateTime? sunday, string searchInput, string doctorId)
        {
            //////////////////
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("login_or_register", "account");
            }
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = await _userManager.IsInRoleAsync(user, "doctor");
            ViewBag.IsPatient = await _userManager.IsInRoleAsync(user, "patient");
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "administrator");
            //////////////////

            // List of appointments for the logged-in user
            var appointments = _context.Appointments
        .Where(a => a.PatientId == user.Id.ToString())
        .ToList();

            var sortedAppointments = appointments
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();
            ViewBag.Appointments = sortedAppointments;

            // Dates
            if (sunday == null)
            {
                sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            }
            var saturday = ((DateTime)sunday).AddDays(6);

            // Récupérer les rendez-vous du patient et filtrer en mémoire
            List<Appointment> existingAppointments = _context.Appointments
                .Where(a => a.PatientId == user.Id.ToString())
                .ToList()
                .Where(a => DateTime.ParseExact(a.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) >= (DateTime)sunday &&
                            DateTime.ParseExact(a.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) <= saturday)
                .ToList();

            ViewBag.ExistingAppointments = existingAppointments;

            // Patient's information
            ViewBag.PatientId = user.Id;
            ViewBag.PatientLastName = user.LastName;
            ViewBag.PatientFirstName = user.FirstName;

            // List of doctors
            var doctors = _context.Doctors.ToList();
            ViewBag.Doctors = doctors;
            ViewBag.DoctorId = doctorId;

            ViewBag.Sunday = sunday;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> create_appointment(AppointmentInputModel model)
        {
            var id = _context.Appointments.Max(a => a.Id) + 1;

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("login_or_register", "Account");
            }

            var doctor = await _userManager.FindByIdAsync(model.DoctorId);

            Doctor doctor1 = _context.Doctors.Find(model.DoctorId);

            if (model.appointmentDate == null || model.appointmentHour == null || model.DoctorId == null)
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                return RedirectToAction("appointments", "care");
            }

            _logger.LogInformation($"******************************\n");
            _logger.LogInformation($"Id = {id}\n");
            _logger.LogInformation($"Date = {model.appointmentDate}\n");
            _logger.LogInformation($"Time = {model.appointmentHour}\n");
            _logger.LogInformation($"DoctorId = {model.DoctorId}\n");
            _logger.LogInformation($"PatientId = {user.Id}\n");
            _logger.LogInformation($"Specialization = {doctor1.Specialization}\n");
            _logger.LogInformation($"Location = {doctor1.Location}\n");
            _logger.LogInformation($"Status = Pending\n");
            _logger.LogInformation($"DoctorLastName = {doctor.LastName}\n");
            _logger.LogInformation($"DoctorFirstName = {doctor.FirstName}\n");
            _logger.LogInformation($"PatientLastName = {user.LastName}\n");
            _logger.LogInformation($"PatientFirstName = {user.FirstName}\n");
            _logger.LogInformation($"******************************\n");

            var appointment = new Appointment
            {
                Id = id,
                Date = model.appointmentDate,
                Time = model.appointmentHour,
                DoctorId = model.DoctorId,
                PatientId = user.Id,
                Specialization = doctor1.Specialization,
                Location = doctor1.Location,
                Status = "Pending",
                DoctorLastName = doctor.LastName,
                DoctorFirstName = doctor.FirstName,
                PatientLastName = user.LastName,
                PatientFirstName = user.FirstName
            };

            _context.Appointments.Add(appointment);

            _logger.LogInformation($"******************************\nAppointment created for patient {user.UserName} with doctor {doctor.UserName} on {model.appointmentDate} at {model.appointmentHour}.\n******************************\n");

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment booked successfully.";
            return RedirectToAction("appointments", "care");
        }
    }
}