#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthApp.MVC.Models;
using HealthApp.Domain.Data;
using HealthApp.Domain.Migrations;

namespace HealthApp.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly SendEmailModel _sendEmailModel;

        public AdminController(SignInManager<User> signInManager,
            ILogger<AdminController> logger, UserManager<User> userManager, ApplicationDbContext context, SendEmailModel sendEmailModel)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _context = context;
            _sendEmailModel = sendEmailModel;
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
         * This method is used to display the admin panel.
         */
        public IActionResult panel()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;

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

            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");
            return View();
        }

        /*****************************************/

        /*
         * This method is used to display the appointments page.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public IActionResult appointments([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;

            var patients = _context.Patients.ToList();
            var doctors = _context.Doctors.ToList();

            ViewBag.Patients = patients;
            ViewBag.Doctors = doctors;

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

            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            if (searchInput == null) 
            {
                searchInput = "";
            }
            if (searchField == null)
            {
                searchField = "";
            }

            // Get all appointments
            var appointments = _context.Appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            // Search among appointments with the searchInput and searchField
            appointments = searchAppointments(searchInput, searchField);

            ViewBag.Appointments = appointments;

            return View();
        }

        /*
         * This method is used to search for appointments.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public List<Appointment> searchAppointments(string searchInput, string searchField)
        {
            var appointments = _context.Appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            searchInput = searchInput.ToLower();

            if (searchField == "Doctor")
            {
                appointments = appointments.Where(a => a.DoctorFirstName.ToLower().Contains(searchInput) || a.DoctorLastName.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Patient")
            {
                appointments = appointments.Where(a => a.PatientFirstName.ToLower().Contains(searchInput) || a.PatientLastName.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Date")
            {
                appointments = appointments.Where(a => a.Date.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Status")
            {
                appointments = appointments.Where(a => a.Status.ToLower().Contains(searchInput)).ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return appointments;
        }

        /*
         * This method is used to create an appointment.
         * model: the input from the form
         */
        public async Task<IActionResult> create_appointment(BookAppointmentInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var id = _context.Appointments.Max(a => a.Id) + 1;

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                var doctor = await _userManager.FindByIdAsync(model.DoctorId);
                var patient = await _userManager.FindByIdAsync(model.PatientId);

                // Create the appointment
                var appointment = new Appointment
                {
                    Id = id,
                    Date = model.appointmentDate,
                    Time = model.appointmentHour,
                    Status = model.Status,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    DoctorFirstName = doctor.FirstName,
                    DoctorLastName = doctor.LastName,
                    PatientFirstName = patient.FirstName,
                    PatientLastName = patient.LastName,
                    Specialization = model.Specialization,
                    Location = model.Location
                };
                try
                {
                    // Add the appointment to the database
                    _context.Appointments.Add(appointment);

                    // Create notifications for the patient and doctor
                    var patientNotification = new Notification
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        SenderId = user.Id,
                        ReceiverId = model.PatientId,
                        Title = "Appointment Created",
                        Content = $"Your appointment has been created for {model.appointmentDate} at {model.appointmentHour} by an administrator.",
                        IsRead = false
                    };
                    var doctorNotification = new Notification
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        SenderId = user.Id,
                        ReceiverId = model.DoctorId,
                        Title = "Appointment Created",
                        Content = $"Your appointment has been created for {model.appointmentDate} at {model.appointmentHour} by an administrator.",
                        IsRead = false
                    };

                    // Add the notifications to the database
                    _context.Notifications.Add(patientNotification);
                    _context.Notifications.Add(doctorNotification);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Appointment created successfully.";
                    return RedirectToAction("appointments", "admin");
                }
                catch (Exception e)
                {
                    _logger.LogError($"******************************\nError when creating appointment: {e.Message}\n******************************\n");
                    TempData["ErrorMessage"] = $"Error when creating appointment: {e.Message}\n";
                    return RedirectToAction("appointments", "admin");
                }
            }
            else
            {
                _logger.LogError($"******************************\n");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error when creating appointment: {error.ErrorMessage}\n");
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("appointments", "admin");
            }
        }

        /*
         * This method is used to edit an appointment.
         * model: the input from the form
         * id: the id of the appointment
         */
        public async Task<IActionResult> edit_appointment(EditAppointmentInputModel model, int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Get the appointment
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "admin");
            }

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                // Edit the appointment
                appointment.Date = model.Date;
                appointment.Time = model.Hour;
                appointment.Status = model.Stat;

                try
                {
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();

                    // Create notifications for the patient and doctor
                    var patientNotification = new Notification
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        SenderId = user.Id,
                        ReceiverId = model.PId,
                        Title = "Appointment Updated",
                        Content = $"Your appointment has been updated to {model.Date} at {model.Hour} by an administrator.",
                        IsRead = false
                    };

                    var doctorNotification = new Notification
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        SenderId = user.Id,
                        ReceiverId = model.DId,
                        Title = "Appointment Updated",
                        Content = $"Your appointment has been updated to {model.Date} at {model.Hour} by an administrator.",
                        IsRead = false
                    };

                    _context.Notifications.Add(patientNotification);
                    _context.Notifications.Add(doctorNotification);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Appointment updated successfully.";
                    return RedirectToAction("appointments", "admin");
                }
                catch (Exception e)
                {
                    _logger.LogError($"******************************\nError when updating appointment: {e.Message}\n******************************\n");
                    TempData["ErrorMessage"] = $"Error when updating appointment: {e.Message}\n";
                    return RedirectToAction("appointments", "admin");
                }
            }

            TempData["ErrorMessage"] = "Fill in all the fields.";
            return RedirectToAction("appointments", "admin");
        }

        /*
         * This method is used to delete an appointment.
         * id: the id of the appointment
         */
        public async Task<IActionResult> delete_appointment(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this appointment.";
                return RedirectToAction("appointments", "admin");
            }

            var user = await _userManager.GetUserAsync(User);

            // Get the appointment
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "admin");
            }

            try
            {
                // Delete the appointment
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment deleted successfully.";

                // Create notifications for the patient and doctor
                var doctorNotification = new Notification
                {
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = user.Id,
                    ReceiverId = appointment.DoctorId,
                    Title = "Appointment Cancelled",
                    Content = $"Your appointment on {appointment.Date} at {appointment.Time} has been cancelled by an administrator.",
                    IsRead = false
                };

                var patientNotification = new Notification
                {
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = user.Id,
                    ReceiverId = appointment.PatientId,
                    Title = "Appointment Cancelled",
                    Content = $"Your appointment on {appointment.Date} at {appointment.Time} has been cancelled by an administrator.",
                    IsRead = false
                };

                _context.Notifications.Add(patientNotification);
                _context.Notifications.Add(doctorNotification);
                await _context.SaveChangesAsync();

                return RedirectToAction("appointments", "admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"******************************\nError when deleting appointment: {e.Message}\n******************************\n");
                TempData["ErrorMessage"] = $"Error when deleting appointment: {e.Message}\n";
                return RedirectToAction("appointments", "admin");
            }
        }

        /*****************************************/

        /*
         * This method is used to display the users page.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public async Task<IActionResult> users([FromQuery] string searchInput, [FromQuery] string searchField)
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

            // Get all users
            var users = _context.Users.OfType<User>().ToList();
            var roles = new Dictionary<string, IList<string>>();

            // Get all roles for each user
            foreach (var u in users)
            {
                var r = await _userManager.GetRolesAsync(u);
                roles[u.Id] = r;
            }

            // Search among users with the searchInput and searchField
            if (!string.IsNullOrEmpty(searchInput) && !string.IsNullOrEmpty(searchField))
            {
                searchInput = searchInput.ToLower();

                if (searchField == "Name")
                {
                    users = users.Where(u => u.FirstName.ToLower().Contains(searchInput) || u.LastName.ToLower().Contains(searchInput)).ToList();
                }
                else if (searchField == "Email")
                {
                    users = users.Where(u => u.UserName.ToLower().Contains(searchInput)).ToList();
                }
                else if (searchField == "Role")
                {
                    var filteredIds = roles.Where(r => r.Value.Any(role => role.ToLower().Contains(searchInput))).Select(r => r.Key).ToList();
                    users = users.Where(u => filteredIds.Contains(u.Id)).ToList();
                }
                _logger.LogInformation($"******************************\nAdmin {user.Email} has searched for users with {searchField} containing {searchInput}.\n******************************\n");
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            ViewBag.Users = users;
            ViewBag.UserRoles = roles;

            return View();
        }

        /*
         * This method is used to create a new user.
         * model: the input from the form
         */
        public async Task<IActionResult> create_user(CreateUserInputModel model)
        {
            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                // Check if the user already exists
                var users = _userManager.Users.ToList();

                foreach (var u in users)
                {
                    if (u.UserName == model.Email)
                    {
                        _logger.LogError($"******************************\nUser with email {model.Email} already exists.\n******************************\n");
                        TempData["ErrorMessage"] = $"User with email {model.Email} already exists.\n";
                        return RedirectToAction("users", "admin");
                    }
                }

                // Create the user
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    Password = model.Password
                };

                // Check if the user can be created
                var result = await _userManager.CreateAsync(user, model.Password);
                var roleName = model.Role.ToString();

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Send a confirmation email
                    _sendEmailModel.SendCreateConfirmation(model.FirstName, model.LastName, model.Email, "admin");

                    TempData["SuccessMessage"] = $"New user with Email {model.Email} and role {roleName} has been created.\n";

                    try
                    {
                        // Create a new doctor depending on the chosen role
                        if (roleName == "Doctor")
                        {
                            var doctor = new Doctor
                            {
                                UserId = user.Id
                            };
                            doctor.Notifications = new List<Message>();
                            doctor.Appointments = new List<Appointment>();
                            doctor.Specialization = "General Practician";
                            doctor.Location = "Unknown";
                            doctor.FirstName = user.FirstName;
                            doctor.LastName = user.LastName;

                            _context.Doctors.Add(doctor);
                            _context.SaveChanges();
                            _logger.LogInformation($"******************************\nUser {user.UserName} has been added to the database.\n******************************\n");
                        }
                        else if (roleName == "Patient")
                        {
                            var patient = new Patient
                            {
                                UserId = user.Id
                            };
                            patient.Notifications = new List<Message>();
                            patient.MedicalHistories = new List<MedicalHistory>();
                            patient.Appointments = new List<Appointment>();
                            patient.Prescriptions = new List<Prescription>();
                            patient.FirstName = user.FirstName;
                            patient.LastName = user.LastName;

                            _context.Patients.Add(patient);
                            _context.SaveChanges();
                            _logger.LogInformation($"******************************\nUser {user.UserName} has been added to the database.\n******************************\n");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"******************************\nError when creating user: {e.Message}\n******************************\n");
                        TempData["ErrorMessage"] = $"{e.Message}\n";
                        return RedirectToAction("users", "admin");
                    }

                    return RedirectToAction("users", "admin");
                }
                else
                {
                    _logger.LogError($"******************************\n");
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"Error when creating user: {error.Description}\n");
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                    }
                    _logger.LogError($"******************************\n");
                    return RedirectToAction("users", "admin");
                }
            }
            else
            {
                _logger.LogError($"******************************\n");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error when creating user: {error.ErrorMessage}\n");
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("users", "admin");
            }
        }

        /*
         * This method is used to edit a user.
         * model: the input from the form
         * userId: the id of the user
         */
        public async Task<IActionResult> edit_user(EditUserInputModel model, string userId)
        {
            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var userRole = await _userManager.GetRolesAsync(user);

                if (user != null)
                {
                    // Edit the user
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.Address = model.Address;

                    try
                    {
                        // If the user is a doctor or a patient, remove them from the database
                        if (userRole.Contains("doctor"))
                        {
                            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                            if (doctor != null)
                            {
                                _context.Doctors.Remove(doctor);
                                _context.SaveChanges();
                                _logger.LogInformation($"******************************\nDoctor {user.UserName} has been removed from the Doctor table.\n******************************\n");
                            }
                        }
                        else if (userRole.Contains("patient"))
                        {
                            var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
                            if (patient != null)
                            {
                                _context.Patients.Remove(patient);
                                _context.SaveChanges();
                                _logger.LogInformation($"******************************\nPatient {user.UserName} has been removed from the Patient table.\n******************************\n");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"******************************\nError when updating user: {e.Message}\n******************************\n");
                        TempData["ErrorMessage"] = $"1 {e.Message}\n";
                        return RedirectToAction("users", "admin");
                    }

                    var roleName = model.Role.ToString();

                    await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                    await _userManager.AddToRoleAsync(user, roleName);

                    // Update the user in the database
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        // Send a confirmation email
                        _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, user.UserName, "admin", "information");

                        try
                        {
                            // Create a new doctor or patient depending on the chosen role
                            if (roleName == "Doctor")
                            {
                                var doctor = new Doctor
                                {
                                    UserId = user.Id
                                };
                                doctor.Notifications = new List<Message>();
                                doctor.Appointments = new List<Appointment>();
                                doctor.Specialization = "General Practician";
                                doctor.Location = "Unknown";
                                doctor.FirstName = user.FirstName;
                                doctor.LastName = user.LastName;

                                _context.Doctors.Add(doctor);
                                _context.SaveChanges();
                            }
                            else if (roleName == "Patient")
                            {
                                var patient = new Patient
                                {
                                    UserId = user.Id
                                };
                                patient.Notifications = new List<Message>();
                                patient.MedicalHistories = new List<MedicalHistory>();
                                patient.Appointments = new List<Appointment>();
                                patient.Prescriptions = new List<Prescription>();
                                patient.FirstName = user.FirstName;
                                patient.LastName = user.LastName;
                                _context.Patients.Add(patient);
                                _context.SaveChanges();
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"******************************\nError when updating user: {e.Message}\n******************************\n");
                            TempData["ErrorMessage"] = $"2 {e.Message}\n";
                            return RedirectToAction("users", "admin");
                        }

                        _logger.LogInformation($"******************************\nUser {user.UserName} has been updated.\n******************************\n");
                        TempData["SuccessMessage"] = $"User {user.UserName} has been updated.\n";
                        return RedirectToAction("users", "admin");
                    }
                    else
                    {
                        _logger.LogError($"******************************\n");
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"Error when updating user: {error.Description}\n");
                            TempData["ErrorMessage"] = $"{error.Description}\n";
                        }
                        _logger.LogError($"******************************\n");
                        return RedirectToAction("users", "admin");
                    }
                }
                else
                {
                    _logger.LogError($"******************************\nUser with Id {userId} not found.\n******************************\n");
                    TempData["ErrorMessage"] = $"User with Id {userId} not found.\n";
                    return RedirectToAction("users", "admin");
                }
            }
            else
            {
                _logger.LogError($"******************************\n");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error when updating user: {error.ErrorMessage}\n");
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("users", "admin");
            }
        }

        /*
         * This method is used to disable or enable a user depending on their current status.
         * userId: the id of the user
         */
        public async Task<IActionResult> disable_enable(string userId)
        {
            // Get the user
            var user = await _userManager.FindByIdAsync(userId);

            // Check if the user is an admin
            if (user.UserName == "admin@test.fr")
            {
                TempData["ErrorMessage"] = $"You cannot disable this admin user.\n";
                return RedirectToAction("users", "admin");
            }

            // Check if the user is found
            if (user != null)
            {
                // Check if the user is active
                if (user.IsActive)
                {
                    // Disable the user
                    user.IsActive = false;
                }
                else
                {
                    // Enable the user
                    user.IsActive = true;
                }

                try
                {
                    // Update the user in the database
                    if (!user.IsActive)
                    {
                        // Send a confirmation email
                        _sendEmailModel.SendDisableAccount(user.FirstName, user.LastName, user.UserName);
                    }
                    else
                    {
                        // Send a confirmation email
                        _sendEmailModel.SendEnableAccount(user.FirstName, user.LastName, user.UserName);
                    }
                    _context.Users.Update(user);
                    _context.SaveChanges();
                }
                catch
                {
                    _logger.LogError($"******************************\nError when updating user: {user.UserName}\n******************************\n");
                    TempData["ErrorMessage"] = $"Error when updating user: {user.UserName}\n";
                    return RedirectToAction("users", "admin");
                }

                _logger.LogInformation($"******************************\nUser {user.UserName} has been {(user.IsActive ? "enabled" : "disabled")}.\n******************************\n");
                TempData["SuccessMessage"] = $"User {user.UserName} has been {(user.IsActive ? "enabled" : "disabled")}.\n";
                return RedirectToAction("users", "admin");
            }
            else
            {
                _logger.LogError($"******************************\n");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error when updating user: {error.ErrorMessage}\n");
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("users", "admin");
            }
        }

        /*
         * This method is used to delete a user.
         * userId: the id of the user
         */
        public async Task<IActionResult> delete_user(string userId)
        {
            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                // Get the current user
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);

                // Get the user to delete 
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    // Check if the user is an admin
                    var admin = user.UserName == "admin@test.fr";
                    if (admin)
                    {
                        _logger.LogError($"******************************\nAdmin {currentUser.Email} tried to delete the admin@test.fr user.\n******************************\n");
                        TempData["ErrorMessage"] = $"You cannot delete this admin user.\n";
                        return RedirectToAction("users", "admin");
                    }

                    var userEmail = user.UserName;
                    var userFirstName = user.FirstName;
                    var userLastName = user.LastName;
                    var userRole = await _userManager.GetRolesAsync(user);

                    // Check if the user can be deleted
                    var result = await _userManager.DeleteAsync(user);

                    if (result.Succeeded)
                    {
                        // Send a confirmation email
                        _sendEmailModel.SendDeleteConfirmation(userFirstName, userLastName, userEmail, "admin");

                        // Remove the user from the database depending on their role
                        if (userRole.Contains("doctor"))
                        {
                            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                            if (doctor != null)
                            {
                                _context.Doctors.Remove(doctor);
                                _context.SaveChanges();
                            }
                        }
                        else if (userRole.Contains("patient"))
                        {
                            var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
                            if (patient != null)
                            {
                                _context.Patients.Remove(patient);
                                _context.SaveChanges();
                            }
                        }

                        _logger.LogInformation($"******************************\nUser {userEmail} has been deleted.\n******************************\n");
                        TempData["SuccessMessage"] = $"User {userEmail} has been deleted.\n";
                        return RedirectToAction("users", "admin");
                    }
                    else
                    {
                        _logger.LogError($"******************************\n");
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"Error when deleting user: {error.Description}\n");
                            TempData["ErrorMessage"] = $"{error.Description}\n";
                        }
                        _logger.LogError($"******************************\n");
                        return RedirectToAction("users", "admin");
                    }
                }
                else
                {
                    _logger.LogError($"******************************\nUser with Id {userId} not found.\n******************************\n");
                    TempData["ErrorMessage"] = $"User with Id {userId} not found.\n";
                    return RedirectToAction("users", "admin");
                }
            }
            else
            {
                _logger.LogError($"******************************\n");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error when deleting user: {error.ErrorMessage}\n");
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("users", "admin");
            }
        }

        /*****************************************/

        /*
         * This method is used to display the messages page.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public async Task<IActionResult> messages([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            ViewBag.UserId = user.Id;
            ViewBag.Users = _userManager.Users.ToList();

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            if (user.IsActive == false)
            {
                await _signInManager.SignOutAsync();
                TempData["ErrorMessage"] = "Your account has been disabled. To reactive it, please contact us.";
                return RedirectToAction("login", "account");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            if (searchInput == null)
            {
                searchInput = "";
            }
            if (searchField == null)
            {
                searchField = "";
            }

            // Get all messages
            var messages = search_messages(searchInput, searchField);

            // Get all messages sent from the contact page
            var adminMessages = _context.Messages
                .Where(m => m.ReceiverId == user.Id)
                .ToList();

            var patients = _context.Patients.ToList();
            var doctors = _context.Doctors.ToList();

            ViewBag.Patients = patients;
            ViewBag.Doctors = doctors;
            ViewBag.Messages = messages;
            ViewBag.AdminMessages = adminMessages;

            return View();
        }

        /*
         * This method is used to search for messages.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public List<Message> search_messages(string searchInput, string searchField)
        {
            var messages = _context.Messages
                .OrderByDescending(m => m.Date)
                .ToList();

            searchInput = searchInput.ToLower();

            if (searchField == "Date")
            {
                messages = messages
                    .Where(m => m.Date.ToString().ToLower().Contains(searchInput))
                    .OrderByDescending(m => m.Date)
                    .ToList();
            }
            else if (searchField == "Object")
            {
                messages = messages
                    .Where(m => m.Object.ToLower().Contains(searchInput))
                    .OrderByDescending(m => m.Date)
                    .ToList();
            }
            else if (searchField == "Sender")
            {
                messages = messages
                    .Where(m => m.SenderFirstName.ToLower().Contains(searchInput) || m.SenderLastName.ToLower().Contains(searchInput))
                    .OrderByDescending(m => m.Date)
                    .ToList();
            }
            else if (searchField == "Receiver")
            {
                messages = messages.Where(m => m.ReceiverFirstName.ToLower().Contains(searchInput) || m.ReceiverLastName.ToLower().Contains(searchInput))
                    .OrderByDescending(m => m.Date)
                    .ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return messages;
        }

        /*
         * This method is used to display a message.
         * id: the id of the message
         */
        public IActionResult message(int id)
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
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

            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            // Get the message
            var message = _context.Messages.FirstOrDefault(m => m.Id == id);
            if (message == null)
            {
                TempData["ErrorMessage"] = "Message not found.";
                return RedirectToAction("messages", "admin");
            }
            ViewBag.Message = message;

            return View();
        }

        /*
         * This method is used to send a message.
         * model: the input from the form
         */
        public async Task<IActionResult> send_message(NewMessageInputModel model)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                var receiver = await _userManager.FindByIdAsync(model.ReceiverId);

                // Create the message
                var message = new Message
                {
                    Id = _context.Messages.Max(m => m.Id) + 1,
                    Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                    SenderId = user.Id,
                    SenderFirstName = "Hospital",
                    SenderLastName = "Appointment System",
                    ReceiverId = model.ReceiverId,
                    ReceiverFirstName = receiver.FirstName,
                    ReceiverLastName = receiver.LastName,
                    DoctorId = "79b4a630-45a4-45f2-8fa6-af550e965cce",
                    PatientId = "f27dde93-f22b-4a81-a322-4336fc5232f4",
                    Object = model.Object,
                    Content = model.Message,
                    Type = "New",
                    IsRead = false
                };

                try
                {
                    // Add the message to the database
                    _context.Messages.Add(message);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Message sent successfully to all users.";
                    return RedirectToAction("messages", "admin");
                }
                catch (Exception e)
                {
                    _logger.LogError($"******************************\nError when sending message: {e.Message}\n******************************\n");
                    TempData["ErrorMessage"] = $"Error when sending message: {e.Message}\n";
                    return RedirectToAction("messages", "admin");
                }
            }
            else
            {
                _logger.LogError($"******************************\n");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error when sending message: {error.ErrorMessage}\n");
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("messages", "admin");
            }
        }

        /*
         * This method is used to send a system message to all users.
         * model: the input from the form
         */
        public async Task<IActionResult> system_message(NewMessageInputModel model)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                var users = _userManager.Users.ToList();

                // For each user, create a message
                foreach (var u in users)
                {
                    var message = new Message
                    {
                        Id = _context.Messages.Max(m => m.Id) + 1,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        SenderId = user.Id,
                        SenderFirstName = "Hospital",
                        SenderLastName = "Appointment System",
                        ReceiverId = u.Id,
                        ReceiverFirstName = u.FirstName,
                        ReceiverLastName = u.LastName,
                        DoctorId = "79b4a630-45a4-45f2-8fa6-af550e965cce",
                        PatientId = "f27dde93-f22b-4a81-a322-4336fc5232f4",
                        Object = model.Object,
                        Content = model.Message,
                        Type = "New",
                        IsRead = false
                    };

                    try
                    {
                        // Add the message to the database
                        _context.Messages.Add(message);
                        _context.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"******************************\nError when sending message: {e.Message}\n******************************\n");
                        TempData["ErrorMessage"] = $"Error when sending message: {e.Message}\n";
                        return RedirectToAction("messages", "admin");
                    }
                }

                TempData["SuccessMessage"] = "Message sent successfully to all users.";
                return RedirectToAction("messages", "admin");
            }
            else
            {
                _logger.LogError($"******************************\n");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error when sending message: {error.ErrorMessage}\n");
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("messages", "admin");
            }
        }

        /*
         * This method is used to delete a message.
         * id: the id of the message
         */
        public async Task<IActionResult> delete_message(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this message.";
                return RedirectToAction("messages", "admin");
            }

            // Get the message by id
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                TempData["ErrorMessage"] = "Message not found.";
                return RedirectToAction("messages", "admin");
            }
            try
            {
                // Delete the message
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message deleted successfully.";
                return RedirectToAction("messages", "admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"******************************\nError when deleting message: {e.Message}\n******************************\n");
                TempData["ErrorMessage"] = $"Error when deleting message: {e.Message}\n";
                return RedirectToAction("messages", "admin");
            }
        }

        /*****************************************/

        /*
         * This method is used to display the medical histories page.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public async Task<IActionResult> medical_histories([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);

            // Get all doctors
            var doctors = _context.Doctors.ToList();
            ViewBag.Doctors = doctors;

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            if (user.IsActive == false)
            {
                await _signInManager.SignOutAsync();
                TempData["ErrorMessage"] = "Your account has been disabled. To reactive it, please contact us.";
                return RedirectToAction("login", "account");
            }

            var userRoles = await _signInManager.UserManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            // Get all medical histories
            var medicalHistories = _context.MedicalHistories
                .OrderByDescending(m => m.Date)
                .ToList();

            if (searchInput == null)
            {
                searchInput = "";
            }
            if (searchField == null)
            {
                searchField = "";
            }

            // Search among medical histories with the searchInput and searchField
            medicalHistories = search_medical_histories(searchInput, searchField);

            ViewBag.MedicalHistories = medicalHistories;

            return View();
        }

        /*
         * This method is used to search for medical histories.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public List<MedicalHistory> search_medical_histories(string searchInput, string searchField)
        {
            var medicalHistories = _context.MedicalHistories
                .OrderByDescending(m => m.Date)
                .ToList();

            searchInput = searchInput.ToLower();

            if (searchField == "Date")
            {
                medicalHistories = medicalHistories.Where(m => m.Date.ToString().ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Patient")
            {
                medicalHistories = medicalHistories.Where(m => m.PatientLastName.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Doctor")
            {
                medicalHistories = medicalHistories.Where(m => m.DoctorFirstName.ToLower().Contains(searchInput) || m.DoctorLastName.ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Specialitzation")
            {
                medicalHistories = medicalHistories.Where(m => m.Specialization.ToLower().Contains(searchInput)).ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return medicalHistories;
        }

        /*
         * This method is used to edit a medical history.
         * model: the input from the form
         * id: the id of the medical history
         */
        public async Task<IActionResult> edit_medical_history(EditMedicalHistoryInputModel model, int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Get the medical history by id
            var medical_history = await _context.MedicalHistories.FindAsync(id);

            if (medical_history == null)
            {
                TempData["ErrorMessage"] = "Medical history not found.";
                return RedirectToAction("medical_histories", "admin");
            }

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                // Edit the medical history
                medical_history.Date = model.Date;
                medical_history.Diagnosis = model.Diagnosis;
                medical_history.DoctorId = model.DoctorId;
                medical_history.PatientId = model.PatientId;
                medical_history.Specialization = model.Specialization;
                medical_history.Location = model.Location;

                try
                {
                    // Update the medical history in the database
                    _context.MedicalHistories.Update(medical_history);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Medical history updated successfully.";
                    return RedirectToAction("medical_histories", "admin");
                }
                catch (Exception e)
                {
                    _logger.LogError($"******************************\nError when updating medical history: {e.Message}\n******************************\n");
                    TempData["ErrorMessage"] = $"Error when updating medical history: {e.Message}\n";
                    return RedirectToAction("medical_histories", "admin");
                }
            }

            TempData["ErrorMessage"] = "Fill in all the fields.";
            return RedirectToAction("medical_histories", "admin");
        }

        /*
         * This method is used to delete a medical history.
         * id: the id of the medical history
         */
        public async Task<IActionResult> delete_medical_history(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this medical history.";
                return RedirectToAction("medical_histories", "admin");
            }

            // Get the medical history by id
            var medicalHistory = await _context.MedicalHistories.FindAsync(id);

            if (medicalHistory == null)
            {
                TempData["ErrorMessage"] = "Medical history not found.";
                return RedirectToAction("medical_histories", "admin");
            }
            try
            {
                // Delete the medical history
                _context.MedicalHistories.Remove(medicalHistory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medical history deleted successfully.";
                return RedirectToAction("medical_histories", "admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"******************************\nError when deleting medical history: {e.Message}\n******************************\n");
                TempData["ErrorMessage"] = $"Error when deleting medical history: {e.Message}\n";
                return RedirectToAction("medical_histories", "admin");
            }
        }

        /*****************************************/

        /*
         * This method is used to display the prescriptions page.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public async Task<IActionResult> prescriptions([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);

            // Get all doctors
            var doctors = _context.Doctors.ToList();
            ViewBag.Doctors = doctors;

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            if (user.IsActive == false)
            {
                await _signInManager.SignOutAsync();
                TempData["ErrorMessage"] = "Your account has been disabled. To reactive it, please contact us.";
                return RedirectToAction("login", "account");
            }

            var userRoles = await _signInManager.UserManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            // Get all prescriptions
            var prescriptions = _context.Prescriptions
                .OrderByDescending(m => m.Date)
                .ToList();

            if (searchInput == null)
            {
                searchInput = "";
            }
            if (searchField == null)
            {
                searchField = "";
            }

            // Search among prescriptions with the searchInput and searchField
            prescriptions = search_prescriptions(searchInput, searchField);

            ViewBag.Prescriptions = prescriptions;

            return View();
        }

        /*
         * This method is used to search for prescriptions.
         * searchInput: the input from the search bar
         * searchField: the field to search in
         */
        public List<Prescription> search_prescriptions(string searchInput, string searchField)
        {
            var prescriptions = _context.Prescriptions
                .OrderByDescending(m => m.Date)
                .ToList();

            searchInput = searchInput.ToLower();

            if (searchField == "Date")
            {
                prescriptions = prescriptions.Where(p => p.Date.ToString().ToLower().Contains(searchInput)).ToList();
            }
            else if (searchField == "Patient")
            {
                prescriptions = prescriptions;
            }
            else if (searchField == "Doctor")
            {
                prescriptions = prescriptions;
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return prescriptions;
        }

        /*
         * This method is used to edit a prescription.
         * model: the input from the form
         * id: the id of the prescription
         */
        public async Task<IActionResult> edit_prescription(EditPrescriptionInputModel model, int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Get the prescription by id
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescription not found.";
                return RedirectToAction("prescriptions", "admin");
            }

            // Check if all fields are filled
            if (ModelState.IsValid)
            {
                // Edit the prescription
                prescription.Date = model.Date;
                prescription.DoctorId = model.DoctorId;
                prescription.PatientId = model.PatientId;
                prescription.Name = model.Name;
                prescription.Dosage = model.Dosage;
                prescription.Frequency = model.Frequency;
                prescription.Duration = model.Duration;
                prescription.Pharmacy = model.Pharmacy;

                try
                {
                    // Update the prescription in the database
                    _context.Prescriptions.Update(prescription);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Prescription updated successfully.";
                    return RedirectToAction("prescriptions", "admin");
                }
                catch (Exception e)
                {
                    _logger.LogError($"******************************\nError when updating prescription: {e.Message}\n******************************\n");
                    TempData["ErrorMessage"] = $"Error when updating prescription: {e.Message}\n";
                    return RedirectToAction("prescriptions", "admin");
                }
            }

            TempData["ErrorMessage"] = "Fill in all the fields.";
            return RedirectToAction("prescriptions", "admin");
        }

        /*
         * This method is used to delete a prescription.
         * id: the id of the prescription
         */
        public async Task<IActionResult> delete_prescription(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this prescription.";
                return RedirectToAction("prescriptions", "admin");
            }

            // Get the prescription by id
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescription not found.";
                return RedirectToAction("prescriptions", "admin");
            }
            try
            {
                // Delete the prescription
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Prescription deleted successfully.";
                return RedirectToAction("prescriptions", "admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"******************************\nError when deleting prescription: {e.Message}\n******************************\n");
                TempData["ErrorMessage"] = $"Error when deleting prescription: {e.Message}\n";
                return RedirectToAction("prescriptions", "admin");
            }
        }

        /*****************************************/

        /*
         * This method is used to display the logs page.
         */
        public IActionResult logs()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;

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

            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            _logger.LogInformation($"******************************\nAdmin {user.Email} has accessed the logs page.\n******************************\n");
            return View();
        }       
    }
}