#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthApp.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using HealthApp.MVC.Models;
using System.Data.Entity;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using System.Security.Principal;

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

            if (searchInput == null || searchField == null)
            {
                searchInput = "";
                searchField = "";
            }

            var patients = searchPatients(searchInput, searchField);
            var patientUsers = new Dictionary<string, User>();

            foreach (var patient in patients)
            {
                var u = await _userManager.FindByIdAsync(patient.UserId);
                if (u != null)
                {
                    patientUsers[patient.UserId] = u;
                }
            }

            if (searchInput != "" && searchField != "" && patients.Count == 0)
            {
                TempData["ErrorMessage"] = "No patients found.";
                _logger.LogError($"******************************\nNo patients found while searching for patients with {searchField} containing {searchInput}.\n******************************\n");
            }

            _logger.BeginScope($"******************************\nUser {user.UserName} has searched for patients with {searchField} containing {searchInput}.\n******************************\n");

            ViewBag.Patients = patients;
            ViewBag.PatientUsers = patientUsers;

            return View();
        }

        public List<Patient> searchPatients(string searchInput, string searchField)
        {
            var patients = _context.Patients.ToList();

            if (searchField == "Name")
            {
                patients = patients.Where(p => p.FirstName.ToLower().Contains(searchInput) || p.LastName.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Email")
            {
                patients = patients.Where(p => p.User.UserName.ToLower().Contains(searchInput)).ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return patients;
        }

        // care/patient/{id}.cshtml
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

                var notification = new Notification
                {
                    Id = _context.Notifications.Max(n => n.Id) + 1,
                    Content = $"You have a new prescription for {model.Name} from doctor {model.DoctorId}.",
                    Title = "New Prescription",
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = model.DoctorId,
                    ReceiverId = model.PatientId,
                    IsRead = false
                };

                _context.Prescriptions.Add(prescription);
                _context.Notifications.Add(notification);
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
        public async Task<IActionResult> appointments(DateTime? sunday, string doctorId)
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

            var doctorAppointments = _context.Appointments
                .Where(a => a.DoctorId == user.Id.ToString())
                .ToList();

            var sortedAppointments = appointments
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();
            ViewBag.Appointments = sortedAppointments;

            var sortedDoctorAppointments = doctorAppointments
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();
            ViewBag.DoctorAppointments = sortedDoctorAppointments;

            // List of appointments for all patients
            var allAppointments = _context.Appointments
                .Where(a => a.PatientId != user.Id.ToString())
                .ToList();
            ViewBag.AllAppointments = allAppointments;

            // Dates
            if (sunday == null)
            {
                sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            }
            ViewBag.Sunday = sunday;
            ViewBag.Saturday = sunday.Value.AddDays(6);

            // Patient's information
            ViewBag.Patient = user;

            // List of doctors
            var doctors = _context.Doctors.ToList();
            ViewBag.Doctors = doctors;

            // To select the doctor
            ViewBag.DoctorId = doctorId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> book_appointment(BookAppointmentInputModel model)
        {
            var id = _context.Appointments.Max(a => a.Id) + 1;
            var doctor = await _userManager.FindByIdAsync(model.DoctorId);

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("appointments", "care");
            }

            if (model.appointmentDate == null || model.appointmentHour == null || model.DoctorId == null)
            {
                _logger.LogError($"******************************\n"
                   + $"Date: {model.appointmentDate}\n" +
                   $"Hour: {model.appointmentHour}\n" +
                   $"Doctor: {model.DoctorId} ()\n" +
                   $"Patient: {model.PatientId} ({model.PatientFirstName} {model.PatientLastName})\n" +
                   $"Specialization: {model.Specialization}\n" +
                   $"Location: {model.Location}\n" +
                   $"Status: {model.Status}\n" +
                   $"*******************************\n");

                TempData["ErrorMessage"] = "Please fill in all fields.";
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                return RedirectToAction("appointments", "care", new { doctorId = "" });
            }
            else
            {
                List<Appointment> doctorAppointments = _context.Appointments
                .Where(a => a.DoctorId == model.DoctorId)
                .ToList();

                if (doctorAppointments != null)
                {
                    foreach (var a in doctorAppointments)
                    {
                        if (a.Date == model.appointmentDate && a.Time == model.appointmentHour && a.Status != "Cancelled")
                        {
                            TempData["ErrorMessage"] = "Doctor is not available at this time.";
                            _logger.LogError($"******************************\nPatient {model.PatientId} tried to book an appointment with doctor {model.DoctorId} at {model.appointmentDate} {model.appointmentHour}, but the doctor is not available.\n******************************\n");
                            return RedirectToAction("appointments", "care", new { doctorId = model.DoctorId });
                        }
                    }
                }

                var appointment = new Appointment
                {
                    Id = id,
                    Date = model.appointmentDate,
                    Time = model.appointmentHour,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    Specialization = model.Specialization,
                    Location = model.Location,
                    Status = "Pending",
                    DoctorLastName = doctor.LastName,
                    DoctorFirstName = doctor.FirstName,
                    PatientLastName = model.PatientLastName,
                    PatientFirstName = model.PatientFirstName
                };

                var doctorNotification = new Notification
                {
                    Id = _context.Notifications.Max(n => n.Id) + 1,
                    Content = $"You have a new appointment request from patient {model.PatientFirstName} {model.PatientLastName}.",
                    Title = "New Appointment Request",
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = model.PatientId,
                    ReceiverId = model.DoctorId,
                    IsRead = false
                };

                var patientNotification = new Notification
                {
                    Id = _context.Notifications.Max(n => n.Id) + 2,
                    Content = $"Your appointment request has been sent to doctor {doctor.FirstName} {doctor.LastName}.",
                    Title = "Appointment Request Sent",
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = model.DoctorId,
                    ReceiverId = model.PatientId,
                    IsRead = false
                };

                _context.Notifications.Add(doctorNotification);
                _context.Notifications.Add(patientNotification);
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Appointment booked successfully.";
                _logger.LogInformation($"******************************\nAppointment booked for patient {model.PatientId} with doctor {model.DoctorId}.\n******************************\n");
                return RedirectToAction("appointments", "care", new { doctorId = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> edit_appointment(EditAppointmentInputModel model, int Id, string sunday, string doctorId)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            var appointment = _context.Appointments.Find(model.Id);

            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot reschedule past appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Cannot reschedule this appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            _logger.LogInformation($"******************************\n");
            _logger.LogInformation($"Id = {Id}\n");
            _logger.LogInformation($"Date = {model.Date}\n");
            _logger.LogInformation($"Time = {model.Hour}\n");
            _logger.LogInformation($"DoctorId = {model.DId}\n");
            _logger.LogInformation($"PatientId = {model.PId}\n");
            _logger.LogInformation($"Specialization = {model.Spe}\n");
            _logger.LogInformation($"Location = {model.Loc}\n");
            _logger.LogInformation($"Status = {model.Stat}\n");
            _logger.LogInformation($"PatientLastName = {model.PLastName}\n");
            _logger.LogInformation($"PatientFirstName = {model.PFirstName}\n");
            _logger.LogInformation($"******************************\n");

            if (model.Date == null || model.Hour == null || model.DId == null)
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            List<Appointment> doctorAppointments = _context.Appointments
                .Where(a => a.DoctorId == model.DId)
                .ToList();

            if (doctorAppointments != null)
            {
                foreach (var a in doctorAppointments)
                {
                    if (a.Date == model.Date && a.Time == model.Hour && a.Status != "Cancelled")
                    {
                        TempData["ErrorMessage"] = "Doctor is not available at this time.";
                        _logger.LogError($"******************************\nPatient {model.PId} tried to book an appointment with doctor {model.DId} at {model.Date} {model.Hour}, but the doctor is not available.\n******************************\n");
                        return RedirectToAction("appointments", "care", new { doctorId = model.DId });
                    }
                }
            }

            appointment.Date = model.Date;
            appointment.Time = model.Hour;
            appointment.DoctorId = model.DId;
            appointment.PatientId = model.PId;
            appointment.Specialization = model.Spe;
            appointment.Location = model.Loc;
            appointment.Status = model.Stat;
            appointment.PatientLastName = model.PLastName;
            appointment.PatientFirstName = model.PFirstName;

            _context.Appointments.Update(appointment);

            var doctorNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 1,
                Content = $"Your appointment with patient {appointment.PatientFirstName} {appointment.PatientLastName} has been rescheduled to {model.Date} {model.Hour}.",
                Title = "Appointment Rescheduled",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = model.PId,
                ReceiverId = model.DId,
                IsRead = false
            };

            var patientNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 2,
                Content = $"Your appointment with doctor {appointment.DoctorFirstName} {appointment.DoctorLastName} has been rescheduled to {model.Date} {model.Hour}.",
                Title = "Appointment Rescheduled",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = model.DId,
                ReceiverId = model.PId,
                IsRead = false
            };

            _context.Notifications.Add(doctorNotification);
            _context.Notifications.Add(patientNotification);

            _logger.LogInformation($"******************************\nAppointment with id {Id} rescheduled.\n******************************\n");

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
        }

        [HttpPost]
        public async Task<IActionResult> approve_appointment(int id, string sunday)
        {
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care");
            }

            if (appointment.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Appointment already approved.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            if (appointment.Status != "Pending" && appointment.Status != "Approved")
            {
                TempData["ErrorMessage"] = "Cannot approve this appointment.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            var patientNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 1,
                Content = $"Your appointment with doctor {appointment.DoctorFirstName} {appointment.DoctorLastName} has been approved.",
                Title = "Appointment Approved",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = appointment.DoctorId,
                ReceiverId = appointment.PatientId,
                IsRead = false
            };

            appointment.Status = "Approved";

            _context.Notifications.Add(patientNotification);

            _logger.LogInformation($"******************************\nAppointment with id {id} approved.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment approved successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday });
        }

        [HttpPost]
        public async Task<IActionResult> reject_appointment(int id, string sunday)
        {
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care");
            }

            if (appointment.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Approved appointments cannot be rejected.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            if (appointment.Status != "Pending" && appointment.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Cannot reject this appointment.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            appointment.Status = "Rejected";

            var patientNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 1,
                Content = $"Your appointment with doctor {appointment.DoctorFirstName} {appointment.DoctorLastName} has been rejected.",
                Title = "Appointment Rejected",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = appointment.DoctorId,
                ReceiverId = appointment.PatientId,
                IsRead = false
            };

            _context.Notifications.Add(patientNotification);

            _logger.LogInformation($"******************************\nAppointment with id {id} rejected.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment rejected successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday });
        }

        [HttpPost]
        public async Task<IActionResult> complete_appointment(int id, string sunday)
        {
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care");
            }

            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Appointment already completed.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            if (appointment.Status != "Approved" && appointment.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Cannot complete this appointment.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            appointment.Status = "Completed";

            var mhId = _context.MedicalHistories.Max(mh => mh.Id) + 1;
            var medicalHistory = new MedicalHistory
            {
                Id = mhId,
                Date = appointment.Date,
                Diagnosis = "",
                DoctorId = appointment.DoctorId,
                DoctorFirstName = appointment.DoctorFirstName,
                DoctorLastName = appointment.DoctorLastName,
                PatientId = appointment.PatientId,
                PatientLastName = appointment.PatientLastName,
                Specialization = appointment.Specialization,
                Location = appointment.Location
            };

            var patientNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 1,
                Content = $"Your appointment with doctor {appointment.DoctorFirstName} {appointment.DoctorLastName} has been completed.",
                Title = "Appointment Completed",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = appointment.DoctorId,
                ReceiverId = appointment.PatientId,
                IsRead = false
            };

            _context.MedicalHistories.Add(medicalHistory);
            _context.Notifications.Add(patientNotification);

            _logger.LogInformation($"******************************\nAppointment with id {id} completed.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment completed successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday });
        }

        [HttpPost]
        public async Task<IActionResult> cancel_appointment(int id, string sunday, string doctorId)
        {
            var appointment = _context.Appointments.Find(id);

            var today = DateTime.Today.ToString("yyyy-MM-dd");

            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot cancel past appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            if (appointment.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Appointment already cancelled.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot cancel completed appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            var appointmentDate = DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var daysDifference = (appointmentDate - DateTime.Today).TotalDays;
            if (daysDifference < 1)
            {
                _context.Appointments.Remove(appointment);
                _logger.LogInformation($"******************************\nAppointment with id {id} cancelled.\n******************************\n");
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Appointment cancelled successfully. However, please note that you are within the cancellation period. To avoid any penalties, please contact the doctor directly. For more information, please refer to the cancellation policy.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care");
            }

            var patientNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 1,
                Content = $"Your appointment with doctor {appointment.DoctorFirstName} {appointment.DoctorLastName} has been cancelled.",
                Title = "Appointment Cancelled",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = appointment.DoctorId,
                ReceiverId = appointment.PatientId,
                IsRead = false
            };

            var doctorNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 2,
                Content = $"Your appointment with patient {appointment.PatientFirstName} {appointment.PatientLastName} has been cancelled.",
                Title = "Appointment Cancelled",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = appointment.PatientId,
                ReceiverId = appointment.DoctorId,
                IsRead = false
            };

            _context.Notifications.Add(patientNotification);
            _context.Notifications.Add(doctorNotification);

            appointment.Status = "Cancelled";
            _logger.LogInformation($"******************************\nAppointment with id {id} cancelled.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
        }

        [HttpPost]
        public async Task<IActionResult> declare_unavailability(BookAppointmentInputModel model)
        {
            var id = _context.Appointments.Max(a => a.Id) + 1;
            var doctor = await _userManager.GetUserAsync(User);
            if (doctor == null)
            {
                return RedirectToAction("login_or_register", "account");
            }

            var doctorId = doctor.Id.ToString();

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("appointments", "care");
            }

            var appointments = _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToList();

            if (appointments != null)
            {
                foreach (var a in appointments)
                {
                    if (a.Date == model.appointmentDate && a.Time == model.appointmentHour && a.Status == "Unavailable")
                    {
                        TempData["ErrorMessage"] = "Unavailability already declared for this date and time.";
                        _logger.LogError($"******************************\nDoctor {doctorId} tried to declare unavailability for {model.appointmentDate} {model.appointmentHour}, but it is already declared.\n******************************\n");
                        return RedirectToAction("appointments", "care");
                    }
                }
            }

            if (model.appointmentDate == null || model.appointmentHour == null || doctorId == null || doctorId == "")
            {
                _logger.LogError($"******************************\n"
                   + $"Date: {model.appointmentDate}\n" +
                   $"Hour: {model.appointmentHour}\n" +
                   $"Doctor: {doctorId} ({doctor.FirstName} {doctor.LastName})\n" +
                   $"Patient: {model.PatientId} ({model.PatientFirstName} {model.PatientLastName})\n" +
                   $"Specialization: {model.Specialization}\n" +
                   $"Location: {model.Location}\n" +
                   $"Status: {model.Status}\n" +
                   $"*******************************\n");

                TempData["ErrorMessage"] = "Please fill in all fields.";
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                return RedirectToAction("appointments", "care");
            }
            else
            {
                var appointment = new Appointment
                {
                    Id = id,
                    Date = model.appointmentDate,
                    Time = model.appointmentHour,
                    DoctorId = doctorId,
                    PatientId = model.PatientId,
                    Specialization = model.Specialization,
                    Location = model.Location,
                    Status = "Unavailable",
                    DoctorLastName = doctor.LastName,
                    DoctorFirstName = doctor.FirstName,
                    PatientLastName = model.PatientLastName,
                    PatientFirstName = model.PatientFirstName
                };

                var doctorNotification = new Notification
                {
                    Id = _context.Notifications.Max(n => n.Id) + 1,
                    Content = $"You have declared unavailability for {model.appointmentDate} {model.appointmentHour}.",
                    Title = "Unavailability Declared",
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = doctorId,
                    ReceiverId = doctorId,
                    IsRead = false
                };

                _context.Notifications.Add(doctorNotification);
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Unavailability declared successfully.";
                _logger.LogInformation($"******************************\nUnavailability declared for doctor {doctorId}.\n******************************\n");
                return RedirectToAction("appointments", "care");
            }
        }

        [HttpPost]
        public async Task<IActionResult> cancel_unavailability(int id, string sunday)
        {
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Unavailability not found.";
                return RedirectToAction("appointments", "care");
            }

            if (appointment.Status != "Unavailable")
            {
                TempData["ErrorMessage"] = "Cannot cancel this unavailability.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            var doctorNotification = new Notification
            {
                Id = _context.Notifications.Max(n => n.Id) + 1,
                Content = $"Your unavailability for {appointment.Date} {appointment.Time} has been cancelled.",
                Title = "Unavailability Cancelled",
                Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                SenderId = appointment.DoctorId,
                ReceiverId = appointment.DoctorId,
                IsRead = false
            };

            _context.Notifications.Add(doctorNotification);
            _context.Appointments.Remove(appointment);
            _logger.LogInformation($"******************************\nUnavailability with id {id} cancelled.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Unavailability cancelled successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday });
        }
    }
}