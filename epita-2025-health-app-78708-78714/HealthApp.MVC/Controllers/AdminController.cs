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


        // admin/panel.cshtml
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


        // admin/appointments.cshtml
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

            var appointments = _context.Appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            appointments = searchAppointments(searchInput, searchField);

            ViewBag.Appointments = appointments;

            return View();
        }

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

        public async Task<IActionResult> create_appointment(BookAppointmentInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var id = _context.Appointments.Max(a => a.Id) + 1;

            if (ModelState.IsValid)
            {
                var doctor = await _userManager.FindByIdAsync(model.DoctorId);
                var patient = await _userManager.FindByIdAsync(model.PatientId);

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
                    _context.Appointments.Add(appointment);

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

        public async Task<IActionResult> edit_appointment(EditAppointmentInputModel model, int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "admin");
            }

            if (ModelState.IsValid)
            {
                appointment.Date = model.Date;
                appointment.Time = model.Hour;
                appointment.Status = model.Stat;

                try
                {
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();

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

        public async Task<IActionResult> delete_appointment(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this appointment.";
                return RedirectToAction("appointments", "admin");
            }

            var user = await _userManager.GetUserAsync(User);

            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Appointment not found.";
                return RedirectToAction("appointments", "admin");
            }

            try
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment deleted successfully.";

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


        // admin/users.cshtml
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

            // USERS
            var users = _context.Users.OfType<User>().ToList();
            var roles = new Dictionary<string, IList<string>>();

            foreach (var u in users)
            {
                var r = await _userManager.GetRolesAsync(u);
                roles[u.Id] = r;
            }

            // SEARCH
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

            // END
            ViewBag.Users = users;
            ViewBag.UserRoles = roles;

            return View();
        }

        public async Task<IActionResult> create_user(CreateUserInputModel model)
        {
            if (ModelState.IsValid)
            {
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

                var result = await _userManager.CreateAsync(user, model.Password);
                var roleName = model.Role.ToString();

                if (result.Succeeded)
                {
                    //_logger.LogInformation($"******************************\nNew user with Email {model.Email} and role {roleName} has been created.\n******************************\n");

                    await _userManager.AddToRoleAsync(user, roleName);

                    _sendEmailModel.SendCreateConfirmation(model.FirstName, model.LastName, model.Email, "admin");

                    TempData["SuccessMessage"] = $"New user with Email {model.Email} and role {roleName} has been created.\n";

                    try
                    {
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

        public async Task<IActionResult> edit_user(EditUserInputModel model, string userId)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var userRole = await _userManager.GetRolesAsync(user);

                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.UserName = model.Email;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.Address = model.Address;

                    try
                    {
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

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, user.UserName, "admin", "information");

                        try
                        {
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

        public async Task<IActionResult> disable_enable(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user.UserName == "admin@test.fr")
            {
                TempData["ErrorMessage"] = $"You cannot disable this admin user.\n";
                return RedirectToAction("users", "admin");
            }

            if (user != null)
            {
                if (user.IsActive)
                {
                    user.IsActive = false;
                }
                else
                {
                    user.IsActive = true;
                }

                try
                {
                    if (!user.IsActive)
                    {
                        _sendEmailModel.SendDisableAccount(user.FirstName, user.LastName, user.UserName);
                    }
                    else
                    {
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

        public async Task<IActionResult> delete_user(string userId)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);

                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
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

                    var result = await _userManager.DeleteAsync(user);

                    if (result.Succeeded)
                    {
                        _sendEmailModel.SendDeleteConfirmation(userFirstName, userLastName, userEmail, "admin");

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


        // admin/messages.cshtml
        public IActionResult messages()
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


        // admin/medical_histories.cshtml
        public async Task<IActionResult> medical_histories([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);

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

            medicalHistories = searchMedicalHistories(searchInput, searchField);

            ViewBag.MedicalHistories = medicalHistories;

            return View();
        }

        public List<MedicalHistory> searchMedicalHistories(string searchInput, string searchField)
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

        public async Task<IActionResult> edit_medical_history(EditMedicalHistoryInputModel model, int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var medical_history = await _context.MedicalHistories.FindAsync(id);

            if (medical_history == null)
            {
                TempData["ErrorMessage"] = "Medical history not found.";
                return RedirectToAction("medical_histories", "admin");
            }

            if (ModelState.IsValid)
            {
                medical_history.Date = model.Date;
                medical_history.Diagnosis = model.Diagnosis;
                medical_history.DoctorId = model.DoctorId;
                medical_history.PatientId = model.PatientId;
                medical_history.Specialization = model.Specialization;
                medical_history.Location = model.Location;

                try
                {
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

        public async Task<IActionResult> delete_medical_history(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this medical history.";
                return RedirectToAction("medical_histories", "admin");
            }

            var medicalHistory = await _context.MedicalHistories.FindAsync(id);

            if (medicalHistory == null)
            {
                TempData["ErrorMessage"] = "Medical history not found.";
                return RedirectToAction("medical_histories", "admin");
            }
            try
            {
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


        // admin/prescriptions.cshtml
        public async Task<IActionResult> prescriptions([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);

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

            prescriptions = searchPrescriptions(searchInput, searchField);

            ViewBag.Prescriptions = prescriptions;

            return View();
        }

        public List<Prescription> searchPrescriptions(string searchInput, string searchField)
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

        public async Task<IActionResult> edit_prescription(EditPrescriptionInputModel model, int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescription not found.";
                return RedirectToAction("prescriptions", "admin");
            }

            if (ModelState.IsValid)
            {
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

        public async Task<IActionResult> delete_prescription(int id)
        {
            if (id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this prescription.";
                return RedirectToAction("prescriptions", "admin");
            }

            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescription not found.";
                return RedirectToAction("prescriptions", "admin");
            }
            try
            {
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


        // admin/logs.cshtml
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