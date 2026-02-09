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

        /*
         * This method is used to display the error page.
         */
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

        /*****************************************/

        /*
         * This method is used to display the patients page.
         * searchInput: The input to search for.
         * searchField: The field to search in.
         */
        public async Task<IActionResult> patients([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            if (user.IsActive == false)
            {
                _signInManager.SignOutAsync();
                TempData["ErrorMessage"] = "Your account has been disabled. To reactive it, please contact us.";
                return RedirectToAction("login", "account");
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

        /*
         * This method is used to search for patients.
         * searchInput: The input to search for.
         * searchField: The field to search in.
         */
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

        /*****************************************/

        /*
         * This method is used to display the patient page.
         * id: The id of the patient.
         * appointmentsSearchInput: The input to search for in appointments.
         * appointmentsSearchField: The field to search in appointments.
         * prescriptionsSearchInput: The input to search for in prescriptions.
         * prescriptionsSearchField: The field to search in prescriptions.
         * medicalHistoriesSearchInput: The input to search for in medical histories.
         * medicalHistoriesSearchField: The field to search in medical histories.
         */
        public async Task<IActionResult> patient(string id,
            [FromQuery] string appointmentsSearchInput, [FromQuery] string appointmentsSearchField,
            [FromQuery] string prescriptionsSearchInput, [FromQuery] string prescriptionsSearchField,
            [FromQuery] string medicalHistoriesSearchInput, [FromQuery] string medicalHistoriesSearchField)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            if (user.IsActive == false)
            {
                _signInManager.SignOutAsync();
                TempData["ErrorMessage"] = "Your account has been disabled. To reactive it, please contact us.";
                return RedirectToAction("login", "account");
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

            var appointments = _context.Appointments
                .Where(a => a.PatientId == id)
                .OrderByDescending(a => a.Date)
                .ToList();

            if (appointmentsSearchField != null && appointmentsSearchInput != null)
            {
                appointments = searchAppointment(appointmentsSearchInput, appointmentsSearchField);
            }

            var prescriptions = _context.Prescriptions
                .Where(p => p.PatientId == id)
                .OrderByDescending(p => p.Date)
                .ToList();

            if (prescriptionsSearchField != null && prescriptionsSearchInput != null)
            {
                prescriptions = searchPrescription(prescriptionsSearchInput, prescriptionsSearchField);
            }

            var medicalHistories = _context.MedicalHistories
                .Where(mh => mh.PatientId == id)
                .OrderByDescending(mh => mh.Date)
                .ToList();

            if (medicalHistoriesSearchField != null && medicalHistoriesSearchInput != null)
            {
                medicalHistories = searchMedicalHistory(medicalHistoriesSearchInput, medicalHistoriesSearchField);
            }

            ViewBag.Appointments = appointments;
            ViewBag.Prescriptions = prescriptions;
            ViewBag.MedicalHistories = medicalHistories;

            ViewBag.DoctorId = user.Id;
            ViewBag.PatientId = id;

            return View();
        }

        /*
         * This method is used to search for appointments.
         * searchInput: The input to search for.
         * searchField: The field to search in.
         */
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

            appointments = appointments
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();

            return appointments;
        }

        /*
         * This method is used to search for medical histories.
         * searchInput: The input to search for.
         * searchField: The field to search in.
         */
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

            medicalHistories = medicalHistories
                .OrderByDescending(mh => mh.Date)
                .ToList();

            return medicalHistories;
        }

        /*
         * This method is used to search for prescriptions.
         * searchInput: The input to search for.
         * searchField: The field to search in.
         */
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

            prescriptions = prescriptions
                .OrderByDescending(p => p.Date)
                .ToList();

            return prescriptions;
        }

        /*****************************************/

        /*
         * This method is used to create a new prescription.
         * patientId: The id of the patient.
         * model: The input model for the prescription.
         */
        [HttpPost]
        public async Task<IActionResult> create_prescription(string patientId, PrescriptionInputModel model)
        {
            /// Get the patient
            var patient = await _userManager.FindByIdAsync(patientId);

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                var id = _context.Prescriptions.Max(p => p.Id) + 1;

                // Create a new prescription
                var prescription = new Prescription
                {
                    Id = id,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    Name = model.Name,
                    Dosage = model.Dosage.ToString(),
                    Frequency = model.Frequency.ToString(),
                    Duration = model.Duration.ToString(),
                    Pharmacy = model.Pharmacy,
                    Date = model.Date
                };

                // Get the doctor
                var doctor = await _userManager.FindByIdAsync(model.DoctorId);

                // Create a new notification for the patient
                var notification = new Notification
                {
                    Id = _context.Notifications.Max(n => n.Id) + 1,
                    Content = $"You have a new prescription for {model.Name} from doctor {doctor.FirstName} {doctor.LastName}.",
                    Title = "New Prescription",
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = model.DoctorId,
                    ReceiverId = model.PatientId,
                    IsRead = false
                };

                // Add the prescription and notification to the database
                _context.Prescriptions.Add(prescription);
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"******************************\nPrescription created for patient {patient.Email} by doctor {model.DoctorId}.\n******************************\n");
            }

            return RedirectToAction("patient", "care", new { id = model.PatientId });
        }

        /*****************************************/

        /*
         * This method is used to edit an appointment.
         * model: The input model for the appointment.
         * Id: The id of the appointment.
         */
        [HttpPost]
        public async Task<IActionResult> patient_edit_appointment(EditAppointmentInputModel model, int Id)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            // Get the appointment
            var appointment = _context.Appointments.Find(model.Id);

            var patientId = model.PId;

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot reschedule past appointments.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment is already approved
            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Cannot reschedule this appointments.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            if (model.Date == null || model.Hour == null || model.DId == null)
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("patients", "care");
            }

            // Check if the doctor is available
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
                        return RedirectToAction("patient", "care", new { id = patientId });
                    }
                }
            }

            // Edit the appointment
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

            // Create notifications for the doctor and patient
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

            // Add the notifications to the database
            _context.Notifications.Add(doctorNotification);
            _context.Notifications.Add(patientNotification);

            _logger.LogInformation($"******************************\nAppointment with id {Id} rescheduled.\n******************************\n");

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment rescheduled successfully.";
            return RedirectToAction("patient", "care", new { id = patientId });
        }

        /*
         * This method is used to approve an appointment.
         * id: The id of the appointment.
         */
        [HttpPost]
        public async Task<IActionResult> patient_approve_appointment(int id)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            var today = DateTime.Today.ToString("yyyy-MM-dd");

            var patientId = appointment.PatientId;

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("patients", "care");
            }

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot approve past appointments.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment is already approved
            if (appointment.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Appointment already approved.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment can be approved
            if (appointment.Status != "Pending" && appointment.Status != "Approved")
            {
                TempData["ErrorMessage"] = "Cannot approve this appointment.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Create a new notification for the patient
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
            return RedirectToAction("patient", "care", new { id = patientId });
        }

        /*
         * This method is used to reject an appointment.
         * id: The id of the appointment.
         */
        [HttpPost]
        public async Task<IActionResult> patient_reject_appointment(int id)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            var today = DateTime.Today.ToString("yyyy-MM-dd");

            var patientId = appointment.PatientId;

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("patients", "care");
            }

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot reject past appointments.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment is already approved
            if (appointment.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Approved appointments cannot be rejected.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment is already rejected
            if (appointment.Status == "Rejected")
            {
                TempData["ErrorMessage"] = "Appointment already rejected.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment can be rejected
            if (appointment.Status != "Pending" && appointment.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Cannot reject this appointment.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            appointment.Status = "Rejected";

            // Create a new notification for the patient
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
            return RedirectToAction("patient", "care", new { id = patientId });
        }

        /*
         * This method is used to complete an appointment.
         * id: The id of the appointment.
         */
        [HttpPost]
        public async Task<IActionResult> patient_complete_appointment(int id)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            var patientId = appointment.PatientId;

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("patients", "care");
            }

            // Check if the appointment is already completed
            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Appointment already completed.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment can be completed
            if (appointment.Status != "Approved" && appointment.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Cannot complete this appointment.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            appointment.Status = "Completed";

            // Create a new medical history
            var mhId = _context.MedicalHistories.Max(mh => mh.Id) + 1;
            var medicalHistory = new MedicalHistory
            {
                Id = mhId,
                Date = appointment.Date,
                Diagnosis = "Empty",
                DoctorId = appointment.DoctorId,
                DoctorFirstName = appointment.DoctorFirstName,
                DoctorLastName = appointment.DoctorLastName,
                PatientId = appointment.PatientId,
                PatientLastName = appointment.PatientLastName,
                Specialization = appointment.Specialization,
                Location = appointment.Location
            };

            // Create a new notification for the patient
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

            // Add the medical history and notification to the database
            _context.MedicalHistories.Add(medicalHistory);
            _context.Notifications.Add(patientNotification);

            _logger.LogInformation($"******************************\nAppointment with id {id} completed.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment completed successfully.";
            return RedirectToAction("patient", "care", new { id = patientId });
        }

        /*
         * This method is used to cancel an appointment.
         * id: The id of the appointment.
         */
        [HttpPost]
        public async Task<IActionResult> patient_cancel_appointment(int id)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("patients", "care");
            }

            var patientId = appointment.PatientId;

            var today = DateTime.Today.ToString("yyyy-MM-dd");

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot cancel past appointments.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment is already cancelled
            if (appointment.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Appointment already cancelled.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment is already completed
            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot cancel completed appointments.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Check if the appointment can be cancelled
            var appointmentDate = DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            // Calculate the difference in days from today to the appointment date
            var daysDifference = (appointmentDate - DateTime.Today).TotalDays;
            // Check if the appointment is within the cancellation period
            if (daysDifference < 1)
            {
                _context.Appointments.Remove(appointment);
                _logger.LogInformation($"******************************\nAppointment with id {id} cancelled.\n******************************\n");
                await _context.SaveChangesAsync();
                TempData["WarningMessage"] = "Appointment cancelled successfully. However, please note that you are within the cancellation period. To avoid any penalties, please contact the doctor directly. For more information, please refer to the cancellation policy.";
                return RedirectToAction("patient", "care", new { id = patientId });
            }

            // Create a new notification for the patient and doctor
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
            return RedirectToAction("patient", "care", new { id = patientId });
        }

        /*****************************************/

        /*
         * This method is used to create a new medical history.
         * model: The input model for the medical history.
         */
        [HttpPost]
        public async Task<IActionResult> edit_medical_history(MedicalHistoryInputModel model, int id)
        {
            // Get the medical history
            var medicalHistory = _context.MedicalHistories.Find(id);

            if (medicalHistory == null) 
            {
                TempData["ErrorMessage"] = "Medical history not found.";
                return RedirectToAction("patients", "care");
            }

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                // Get the diagnosis from the model
                medicalHistory.Diagnosis = model.Diagnosis;

                // Update the diagnosis in the database
                _context.MedicalHistories.Update(medicalHistory);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"******************************\nMedical history with id {id} updated.\n******************************\n");
                TempData["SuccessMessage"] = "Medical history updated successfully.";
                return RedirectToAction("patient", "care", new { id = medicalHistory.PatientId });
            }
            _logger.LogError($"******************************\nFailed to update medical history with id {id}.\n******************************\n");
            TempData["ErrorMessage"] = "Please fill in all fields.";
            return RedirectToAction("patient", "care", new { id = medicalHistory.PatientId });
        }

        /*****************************************/

        /*
         * This method is used to display the doctors page.
         * searchInput: The input to search for.
         * searchField: The field to search in.
         */
        public async Task<IActionResult> doctors([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return RedirectToAction("login_or_register", "account");
            }

            if (user.IsActive == false)
            {
                _signInManager.SignOutAsync();
                TempData["ErrorMessage"] = "Your account has been disabled. To reactive it, please contact us.";
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

            // Search for doctors with the given input and field
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

            _logger.LogInformation($"******************************\nUser {user.UserName} has searched for doctors with {searchField} containing {searchInput}.\n******************************\n");

            ViewBag.Doctors = doctors;
            ViewBag.DoctorUsers = doctorUsers;

            return View();
        }

        /*
         * This method is used to search for doctors.
         * searchInput: The input to search for.
         * searchField: The field to search in.
         */
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

        /*****************************************/

        /*
         * This method is used to display the appointments page.
         * sunday: The date of the week.
         * doctorId: The id of the doctor.
         */
        public async Task<IActionResult> appointments(DateTime? sunday, string doctorId)
        {
            //////////////////
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            if (user.IsActive == false)
            {
                _signInManager.SignOutAsync();
                TempData["ErrorMessage"] = "Your account has been disabled. To reactive it, please contact us.";
                return RedirectToAction("login", "account");
            }

            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = await _userManager.IsInRoleAsync(user, "doctor");
            ViewBag.IsPatient = await _userManager.IsInRoleAsync(user, "patient");
            ViewBag.IsAdmin = await _userManager.IsInRoleAsync(user, "administrator");
            //////////////////

            // Get the appointments for the logged-in user
            var appointments = _context.Appointments
                .Where(a => a.PatientId == user.Id.ToString())
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();

            // Get the appointments for the logged-in doctor's patients
            var doctorAppointments = _context.Appointments
                .Where(a => a.DoctorId == user.Id.ToString())
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();

            ViewBag.Appointments = appointments;
            ViewBag.DoctorAppointments = doctorAppointments;

            // Get all appointments for the other patients
            var allAppointments = _context.Appointments
                .Where(a => a.PatientId != user.Id.ToString())
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();
            ViewBag.AllAppointments = allAppointments;

            // Get the date of the week
            if (sunday == null)
            {
                sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            }
            ViewBag.Sunday = sunday;
            ViewBag.Saturday = sunday.Value.AddDays(6);

            ViewBag.Patient = user;

            // Get the list of doctors
            var doctors = _context.Doctors.ToList();
            ViewBag.Doctors = doctors;
            
            ViewBag.DoctorId = doctorId;

            return View();
        }

        /*
         * This method is used to book an appointment.
         * model: The input model for the appointment.
         */
        [HttpPost]
        public async Task<IActionResult> book_appointment(BookAppointmentInputModel model)
        {
            var id = _context.Appointments.Max(a => a.Id) + 1;
            var doctor = await _userManager.FindByIdAsync(model.DoctorId);
            var patient = await _userManager.FindByIdAsync(model.PatientId);

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("appointments", "care");
            }

            if (model.appointmentDate == null || model.appointmentHour == null || model.DoctorId == null)
            {
                TempData["ErrorMessage"] = "Please fill in all fields.";
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                return RedirectToAction("appointments", "care", new { doctorId = "" });
            }
            else
            {
                // Get the appointments for the doctor
                List<Appointment> doctorAppointments = _context.Appointments
                    .Where(a => a.DoctorId == model.DoctorId)
                    .OrderByDescending(a => a.Date)
                    .ThenBy(a => a.Time)
                    .ToList();

                // Check if the doctor is available
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

                // Create a new appointment
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
                    PatientLastName = patient.LastName,
                    PatientFirstName = patient.FirstName
                };

                // Create notifications for the doctor and patient
                var doctorNotification = new Notification
                {
                    Id = _context.Notifications.Max(n => n.Id) + 1,
                    Content = $"You have a new appointment request from patient {patient.FirstName} {patient.LastName}.",
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

        /*
         * This method is used to edit an appointment.
         * model: The input model for the appointment.
         * Id: The id of the appointment.
         * sunday: The date of the week.
         * doctorId: The id of the doctor.
         */
        [HttpPost]
        public async Task<IActionResult> edit_appointment(EditAppointmentInputModel model, int Id, string sunday, string doctorId)
        {
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            // Get the appointment
            var appointment = _context.Appointments.Find(model.Id);

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot reschedule past appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            // Check if the appointment is already approved
            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Cannot reschedule this appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

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

            // Check if the doctor is available
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

            // Edit the appointment
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

            // Create notifications for the doctor and patient
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

        /*
         * This method is used to approve an appointment.
         * id: The id of the appointment.
         * sunday: The date of the week.
         */
        [HttpPost]
        public async Task<IActionResult> approve_appointment(int id, string sunday)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            var today = DateTime.Today.ToString("yyyy-MM-dd");

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care");
            }

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot approve past appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the appointment is already approved
            if (appointment.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Appointment already approved.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the appointment can be approved
            if (appointment.Status != "Pending" && appointment.Status != "Approved")
            {
                TempData["ErrorMessage"] = "Cannot approve this appointment.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Create a new notification for the patient
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

        /*
         * This method is used to reject an appointment.
         * id: The id of the appointment.
         * sunday: The date of the week.
         */
        [HttpPost]
        public async Task<IActionResult> reject_appointment(int id, string sunday)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            var today = DateTime.Today.ToString("yyyy-MM-dd");

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care");
            }

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot reject past appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the appointment is already approved
            if (appointment.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Approved appointments cannot be rejected.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the appointment is already rejected
            if (appointment.Status == "Rejected")
            {
                TempData["ErrorMessage"] = "Appointment already rejected.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the appointment can be rejected
            if (appointment.Status != "Pending" && appointment.Status != "Rejected")
            {
                TempData["ErrorMessage"] = "Cannot reject this appointment.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            appointment.Status = "Rejected";

            // Create a new notification for the patient
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

        /*
         * This method is used to complete an appointment.
         * id: The id of the appointment.
         * sunday: The date of the week.
         */
        [HttpPost]
        public async Task<IActionResult> complete_appointment(int id, string sunday)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "care");
            }

            // Check if the appointment is already completed
            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Appointment already completed.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the appointment can be completed
            if (appointment.Status != "Approved" && appointment.Status != "Completed")
            {
                TempData["ErrorMessage"] = "Cannot complete this appointment.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            appointment.Status = "Completed";

            // Create a new medical history
            var mhId = _context.MedicalHistories.Max(mh => mh.Id) + 1;
            var medicalHistory = new MedicalHistory
            {
                Id = mhId,
                Date = appointment.Date,
                Diagnosis = "Empty",
                DoctorId = appointment.DoctorId,
                DoctorFirstName = appointment.DoctorFirstName,
                DoctorLastName = appointment.DoctorLastName,
                PatientId = appointment.PatientId,
                PatientFirstName = appointment.PatientFirstName,
                PatientLastName = appointment.PatientLastName,
                Specialization = appointment.Specialization,
                Location = appointment.Location
            };

            // Create a new notification for the patient
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

            // Add the medical history and notification to the database
            _context.MedicalHistories.Add(medicalHistory);
            _context.Notifications.Add(patientNotification);

            _logger.LogInformation($"******************************\nAppointment with id {id} completed.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment completed successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday });
        }

        /*
         * This method is used to cancel an appointment.
         * id: The id of the appointment.
         * sunday: The date of the week.
         * doctorId: The id of the doctor.
         */
        [HttpPost]
        public async Task<IActionResult> cancel_appointment(int id, string sunday, string doctorId)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            var today = DateTime.Today.ToString("yyyy-MM-dd");

            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot cancel past appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            // Check if the appointment is already cancelled
            if (appointment.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Appointment already cancelled.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            // Check if the appointment is already completed
            if (appointment.Status == "Completed")
            {
                TempData["ErrorMessage"] = "Cannot cancel completed appointments.";
                return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
            }

            var appointmentDate = DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            // Calculate the difference in days from today to the appointment date
            var daysDifference = (appointmentDate - DateTime.Today).TotalDays;
            // Check if the appointment is within the cancellation period
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

            // Create a new notification for the patient and doctor
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

            // Add the notifications to the database
            _context.Notifications.Add(patientNotification);
            _context.Notifications.Add(doctorNotification);

            appointment.Status = "Cancelled";
            _logger.LogInformation($"******************************\nAppointment with id {id} cancelled.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday, doctorId = doctorId });
        }

        /*
         * This method is used to declare unavailability.
         * model: The input model for the appointment.
         */
        [HttpPost]
        public async Task<IActionResult> declare_unavailability(BookAppointmentInputModel model)
        {
            var id = _context.Appointments.Max(a => a.Id) + 1;
            var doctor = await _userManager.GetUserAsync(User);
            var patient = await _userManager.FindByIdAsync(model.PatientId);

            if (doctor == null)
            {
                return RedirectToAction("login_or_register", "account");
            }

            var doctorId = doctor.Id;

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("appointments", "care");
            }

            // Get the appointments for the doctor
            var appointments = _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToList();

            // Check if the doctor is available
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
                TempData["ErrorMessage"] = "Please fill in all fields.";
                var sunday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                return RedirectToAction("appointments", "care");
            }
            else
            {
                // Create a new appointment for unavailability
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
                    PatientLastName = patient.LastName,
                    PatientFirstName = patient.FirstName
                };

                // Create a new notification for the doctor
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

                // Add the appointment and notification to the database
                _context.Notifications.Add(doctorNotification);
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Unavailability declared successfully.";
                _logger.LogInformation($"******************************\nUnavailability declared for doctor {doctorId}.\n******************************\n");
                return RedirectToAction("appointments", "care");
            }
        }

        /*
         * This method is used to cancel unavailability.
         * id: The id of the appointment.
         * sunday: The date of the week.
         */
        [HttpPost]
        public async Task<IActionResult> cancel_unavailability(int id, string sunday)
        {
            // Get the appointment
            var appointment = _context.Appointments.Find(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Unavailability not found.";
                return RedirectToAction("appointments", "care");
            }

            var today = DateTime.Today.ToString("yyyy-MM-dd");
            // Check if the appointment is in the past
            if (DateTime.ParseExact(appointment.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) < DateTime.ParseExact(today, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture))
            {
                TempData["ErrorMessage"] = "Cannot cancel past unavailability.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the availability can be cancelled
            if (appointment.Status != "Unavailable")
            {
                TempData["ErrorMessage"] = "Cannot cancel this unavailability.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Check if the appointment is already cancelled
            if (appointment.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Unavailability already cancelled.";
                return RedirectToAction("appointments", "care", new { sunday = sunday });
            }

            // Create a new notification for the doctor
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

            // Add the notification and remove the appointment from the database
            _context.Notifications.Add(doctorNotification);
            _context.Appointments.Remove(appointment);
            _logger.LogInformation($"******************************\nUnavailability with id {id} cancelled.\n******************************\n");
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Unavailability cancelled successfully.";
            return RedirectToAction("appointments", "care", new { sunday = sunday });
        }
    }
}