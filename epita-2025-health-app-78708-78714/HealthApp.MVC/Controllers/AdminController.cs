#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthApp.MVC.Models;
using HealthApp.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        public IActionResult panel()
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

        public IActionResult appointments()
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

        public async Task<IActionResult> users([FromQuery] string searchInput, [FromQuery] string searchField)
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

        public IActionResult messages()
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

        public IActionResult logs()
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

            _logger.LogInformation($"******************************\nAdmin {user.Email} has accessed the logs page.\n******************************\n");
            return View();
        }

        public async Task<IActionResult> create_user(CreateUserInputModel model)
        {
            if (ModelState.IsValid)
            {
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
                            doctor.Notifications = new List<Notification>();
                            doctor.Appointments = new List<Appointment>();
                            doctor.Specialization = "";
                            doctor.Location = "";

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
                            patient.Notifications = new List<Notification>();
                            patient.MedicalHistories = new List<MedicalHistory>();
                            patient.Appointments = new List<Appointment>();
                            patient.Prescriptions = new List<Prescription>();

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
                                doctor.Notifications = new List<Notification>();
                                doctor.Appointments = new List<Appointment>();
                                doctor.Specialization = "";
                                doctor.Location = "";

                                _context.Doctors.Add(doctor);
                                _context.SaveChanges();
                            }
                            else if (roleName == "Patient")
                            {
                                var patient = new Patient
                                {
                                    UserId = user.Id
                                };
                                patient.Notifications = new List<Notification>();
                                patient.MedicalHistories = new List<MedicalHistory>();
                                patient.Appointments = new List<Appointment>();
                                patient.Prescriptions = new List<Prescription>();
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
    }
}