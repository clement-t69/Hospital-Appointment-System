#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net;
using HealthApp.Domain.Data;

namespace HealthApp.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly SendEmailModel _sendEmailModel;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        private ChangeEmailInputModel changeEmailModel = new ChangeEmailInputModel();
        private ChangePasswordInputModel changePasswordModel = new ChangePasswordInputModel();

        public AccountController(SignInManager<User> signInManager,
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger, SendEmailModel sendEmailModel, IHttpContextAccessor httpContextAccessor
            , ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _sendEmailModel = sendEmailModel;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        // ERROR
        public IActionResult error()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            _logger.LogError($"******************************\nUser {user.Email} has encountered an error. (redirected to error page)\n******************************\n");
            return View();
        }

        // HEADER
        // IF NOT LOGGED IN
        public IActionResult login_or_register()
        {
            return View();
        }

        // IF LOGGED IN
        public IActionResult my_messages()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            return View();
        }


        public IActionResult send_message()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");
            var doctors = _context.Doctors.ToList();
            ViewBag.Doctors = doctors;
            return View();
        }

        public IActionResult my_appointments()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            return RedirectToAction("appointments", "care");

        }
        
        public IActionResult edit()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> logout()
        {
            User user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"******************************\nUser {user.Email} has logged out.\n******************************\n");

            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        // FOOTER
        public IActionResult cancellation_policy()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            return View();
        }

        // LOGIN
        public IActionResult login()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> login(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"******************************\nUser {model.Email} has logged in.\n******************************\n");
                    return RedirectToAction("edit", "account");
                }
                else
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to log in.\n******************************\n");
                    TempData["ErrorMessage"] = "Wrong credentials.\n";
                    return View(model);
                }
            }

            _logger.LogError($"******************************\nUser {model.Email} has failed to log in.\n******************************\n");
            TempData["ErrorMessage"] = "Wrong credentials.\n";
            return View(model);
        }

        // REGISTER
        public IActionResult register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> register(RegisterInputModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password,
                    Address = model.Address
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                var roleName = "Patient";

                if (result.Succeeded)
                {
                    _logger.LogInformation($"******************************\nUser {model.Email} has registered.\n******************************\n");

                    _sendEmailModel.SendCreateConfirmation(model.FirstName, model.LastName, model.Email, "user");

                    await _signInManager.SignInAsync(user, isPersistent: true);
                    await _userManager.AddToRoleAsync(user, roleName);

                    try
                    {
                        var patient = new Patient
                        {
                            UserId = user.Id
                        };
                        patient.Appointments = new List<Appointment>();
                        patient.MedicalHistories = new List<MedicalHistory>();
                        patient.Notifications = new List<Notification>();
                        patient.Prescriptions = new List<Prescription>();
                        patient.FirstName = user.FirstName;
                        patient.LastName = user.LastName;

                        _context.Patients.Add(patient);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                    }

                    return RedirectToAction("edit", "account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"******************************\nUser {model.Email} has failed to register.\n******************************\n");
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                        return View(model);
                    }
                }
            }
            _logger.LogError($"******************************\nUser {model.Email} has failed to register.\n******************************\n");
            TempData["ErrorMessage"] = "Registration failed.\n";
            return View(model);
        }

        // FORGOT PASSWORD
        public IActionResult forgot_password()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> forgot_password(ForgotPasswordInputModel model)
        {
            if (ModelState.IsValid)
            {
                string emailToLower = model.Email.ToLower();

                var user = await _userManager.FindByEmailAsync(emailToLower);
                if (user == null)
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password: Email not found.\n******************************\n");
                    TempData["ErrorMessage"] = "Email not found.\n";
                    return View(model);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var existingToken = _context.UserTokens.FirstOrDefault(t => t.UserId == user.Id && t.LoginProvider == "Default" && t.Name == "PasswordReset");

                if (existingToken == null)
                {
                    //_logger.LogError($"******************************\nToken {token} has been generated for User {model.Email}.\n******************************\n");
                    try
                    {
                        _context.UserTokens.Add(new IdentityUserToken<string>
                        {
                            UserId = user.Id,
                            LoginProvider = "Default",
                            Name = "PasswordReset",
                            Value = token
                        });

                        //_logger.LogInformation($"******************************\nToken {token} has been inserted manually for User {model.Email}.\n******************************\n");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                    }
                }
                else
                    existingToken.Value = token;

                await _context.SaveChangesAsync();

                var callbackUrl = Url.Action(
                    "reset_password",
                    "account",
                    new { token = WebUtility.UrlEncode(token), email = user.UserName },
                    //new { token, email = user.UserName },
                    Request.Scheme
                );


                _sendEmailModel.SendForgotPassword(user.FirstName, user.LastName, model.Email, token, callbackUrl);

                _logger.LogInformation($"******************************\nUser {model.Email} has requested a Password reset.\n******************************\n");
                TempData["SuccessMessage"] = "An Email has been sent to reset your Password.\n";
                
                return RedirectToAction("login", "account");
            }
            
            _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password.\n******************************\n");
            TempData["ErrorMessage"] = "An error occurred while resetting your Password.\n";
            return View(model);
        }

        public IActionResult reset_password(string token, string email)
        {
            ViewBag.token = WebUtility.UrlDecode(token);
            ViewBag.email = email;

            //_logger.LogInformation($"Recevied Email (GET) : {email}");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> reset_password(ResetPasswordInputModel model)
        {
            //_logger.LogInformation($"Received Email (POST) : {model.Email}");

            if (ModelState.IsValid)
            {
                var decodedToken = WebUtility.UrlDecode(model.Token);
                //var decodedToken = model.Token;

                _logger.LogInformation($"******************************\nEmail: {model.Email}\n******************************\n");

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password: Email not found.\n******************************\n");
                    TempData["ErrorMessage"] = "Email not found.\n";
                    return RedirectToAction("forgot_password", "account");
                }

                _logger.LogInformation($"******************************\nEmail: {user.UserName}\nPassword: {model.NewPassword}\n******************************\n");
                
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"******************************\nUser {model.Email} has reset their Password.\n******************************\n");
                    TempData["SuccessMessage"] = "Your Password has been reset.\n";

                    _context.UserTokens.RemoveRange(_context.UserTokens.Where(t => t.UserId == user.Id));
                    await _context.SaveChangesAsync();

                    return RedirectToAction("password_changed", "account");
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password: {error.Description}\n******************************\n");
                    TempData["ErrorMessage"] = $"{error.Description}\n";
                }
            }
            return View(model);
        }

        public IActionResult password_changed()
        {
            return View();
        }

        // EDIT ACCOUNT
        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("login", "account");
            }

            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            ViewBag.UserEmail = user.UserName;
            ViewBag.UserPasswordLength = user.Password.Length;

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            return View(new EditAccountViewModel
            {
                ChangeEmail = new ChangeEmailInputModel(),
                ChangePassword = new ChangePasswordInputModel()
            });
        }

        // ACCESS TO PROFILE, MEDICAL HISTORY, PRESCRIPTIONS
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> my_profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            ViewBag.Phone = user.Phone;
            ViewBag.Address = user.Address;

            if ((await _userManager.GetRolesAsync(user)).Contains("doctor") || (await _userManager.GetRolesAsync(user)).Contains("Doctor"))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);

                if (doctor != null)
                {
                    ViewBag.DoctorSpecialization = doctor.Specialization;
                    ViewBag.DoctorLocation = doctor.Location;
                }
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> change_info(ChangeInfoInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);

            if (patient != null)
            {
                user.Phone = model.Phone;
                user.Address = model.Address;
            }
            else if (doctor != null)
            {
                user.Phone = model.Phone;
                user.Address = model.Address;

                if (string.IsNullOrEmpty(model.Location) || string.IsNullOrEmpty(model.Specialization))
                {
                    TempData["ErrorMessage"] = "Location and Specialization cannot be empty.";
                    return RedirectToAction("my_profile", "account");
                }

                doctor.Specialization = model.Specialization;
                doctor.Location = model.Location;
            }         

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _context.SaveChanges();
                _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, user.UserName, "user", "information");

                _logger.LogInformation($"******************************\nUser {user.Email} has updated their information.\n******************************\n");
                TempData["SuccessMessage"] = "Your information has been updated.\n";
                return RedirectToAction("my_profile", "account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"******************************\nUser {user.Email} has failed to update their information.\n******************************\n");
                    TempData["ErrorMessage"] = $"{error.Description}\n";
                }
            }
            return RedirectToAction("my_profile", "account");
        }

        [HttpGet]
        [Authorize]
        public IActionResult my_medical_history()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient != null)
            {
                var medicalHistories = _context.MedicalHistories.Where(mh => mh.PatientId == patient.UserId).ToList();
                ViewBag.MedicalHistories = medicalHistories;
            }

            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult my_prescriptions()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient != null)
            {
                var prescriptions = _context.Prescriptions.Where(p => p.PatientId == patient.UserId).ToList();
                ViewBag.Prescriptions = prescriptions;
            }

            return View();
        }

        // CHANGE EMAIL
        [HttpPost]
        public async Task<IActionResult> change_email(ChangeEmailInputModel model)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            if (model.CurrentEmail == null || model.NewEmail == null || model.ConfirmNewEmail == null)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: All fields are required.\n******************************\n");
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.CurrentEmail != User.Identity.Name)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: The current Email does not match their account Email.\n******************************\n");
                TempData["ErrorMessage"] = "The current Email does not match your account Email.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewEmail == model.CurrentEmail || model.ConfirmNewEmail == model.CurrentEmail)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: The new Email must be different from the current Email.\n******************************\n");
                TempData["ErrorMessage"] = "The new Email must be different from the current Email.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewEmail != model.ConfirmNewEmail)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: The Emails do not match.\n******************************\n");
                TempData["ErrorMessage"] = "The Emails do not match.\n";
                return RedirectToAction("edit", "account");
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);

                if (existingUser != null && existingUser.Id != user.Id)
                {
                    _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: This Email is already in use.\n******************************\n");
                    TempData["ErrorMessage"] = "This Email is already in use.\n";
                    return RedirectToAction("edit", "account");
                }

                user.Email = model.NewEmail;
                user.NormalizedEmail = model.NewEmail.ToUpper();
                user.UserName = model.NewEmail;
                user.NormalizedUserName = model.NewEmail.ToUpper();

                var emailResult = await _userManager.UpdateAsync(user);

                if (!emailResult.Succeeded)
                {
                    _logger.LogError($"******************************\n");
                    foreach (var error in emailResult.Errors)
                    {
                        _logger.LogError($"User {user.Email} has failed to update their Email: {error.Description}\n");
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                    }
                    _logger.LogError($"******************************\n");
                    return RedirectToAction("edit", "account");
                }

                _logger.LogInformation($"******************************\nUser {user.Email} has updated their Email to {model.NewEmail}.\n******************************\n");

                _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, model.CurrentEmail, "user", "email");

                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your Email has been updated.\n";
                return RedirectToAction("edit", "account");
            }

            _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email.\n******************************\n");
            TempData["ErrorMessage"] = "An error occurred while updating your Email.\n";
            return RedirectToAction("edit", "account");
        }

        // CHANGE PASSWORD
        [HttpPost]
        public async Task<IActionResult> change_password(ChangePasswordInputModel model)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            if (model.CurrentPassword == null || model.NewPassword == null || model.ConfirmNewPassword == null)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: All fields are required.\n******************************\n");
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("edit", "account");
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

            if (!passwordCheck)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: The current Password does not match their account Password.\n******************************\n");
                TempData["ErrorMessage"] = "The current Password does not match your account Password.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewPassword == model.CurrentPassword || model.ConfirmNewPassword == model.CurrentPassword)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: The new Password must be different from the current Password.\n******************************\n");
                TempData["ErrorMessage"] = "The new Password must be different from the current Password.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: The Passwords do not match.\n******************************\n");
                TempData["ErrorMessage"] = "The Passwords do not match.\n";
                return RedirectToAction("edit", "account");
            }

            user.Password = model.NewPassword;
            var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!passwordResult.Succeeded)
            {
                _logger.LogError($"******************************\n");
                foreach (var error in passwordResult.Errors)
                {
                    _logger.LogError($"User {user.Email} has failed to update their Password: {error.Description}\n");
                    TempData["ErrorMessage"] = $"{error.Description}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("edit", "account");
            }

            _logger.LogInformation($"******************************\nUser {user.Email} has updated their Password.\n******************************\n");

            _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, user.UserName, "user", "password");

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your Password has been updated.\n";
            return RedirectToAction("edit", "account");
        }

        // DELETE ACCOUNT BY DELETING ALL INFORMATIONS
        [HttpPost]
        public async Task<IActionResult> delete_all()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);

                if (user != null)
                {
                    var userEmail = user.UserName;
                    var userFirstName = user.FirstName;
                    var userLastName = user.LastName;

                    if (userEmail == "admin@test.fr")
                    {
                        _logger.LogError($"******************************\nUser {userEmail} has failed to delete their account: Admin account cannot be deleted.\n******************************\n");
                        TempData["ErrorMessage"] = "Admin account cannot be deleted.\n";
                        return RedirectToAction("edit", "account");
                    }

                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        try
                        {
                            if (userRoles.Contains("patient"))
                            {
                                var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
                                if (patient != null)
                                {
                                    _context.Patients.Remove(patient);
                                    _context.SaveChanges();
                                }

                                _logger.LogInformation($"******************************\nUser {userEmail} has deleted their account.\n******************************\n");
                            }
                            else if (userRoles.Contains("doctor"))
                            {
                                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                                if (doctor != null)
                                {
                                    _context.Doctors.Remove(doctor);
                                    _context.SaveChanges();
                                }
                                _logger.LogInformation($"******************************\nUser {userEmail} has deleted their account.\n******************************\n");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                        }

                        _sendEmailModel.SendDeleteConfirmation(userFirstName, userLastName, userEmail, "user");

                        await _signInManager.SignOutAsync();
                        return RedirectToAction("index", "home");
                    }
                    else
                    {
                        _logger.LogError($"******************************\n");
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"User {userEmail} has failed to delete their account: {error.Description}\n");
                            TempData["ErrorMessage"] = $"{error.Description}\n";
                        }
                        _logger.LogError($"******************************\n");
                        return View("edit", "account");
                    }
                }
            }
            return RedirectToAction("index", "home");
        }
    }
}