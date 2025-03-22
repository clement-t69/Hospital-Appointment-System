#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using System.Data.Entity.Validation;

namespace HealthApp.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        private ChangeEmailInputModel changeEmailModel = new ChangeEmailInputModel();
        private ChangePasswordInputModel changePasswordModel = new ChangePasswordInputModel();

        public AccountController(SignInManager<User> signInManager,
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // ERROR
        public IActionResult error()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
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
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View();
        }
        public IActionResult my_appointments()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
            return View();
        }
        public IActionResult edit()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        // FOOTER
        public IActionResult cancellation_policy()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
            return View();
        }

        // LOGIN
        public async Task<IActionResult> login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> login(LoginInputModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"{model.Email} logged in.");
                    return RedirectToAction("edit", "account");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                    return View(model);
                }
            }

            return View(model);
        }

        // REGISTER
        public async Task<IActionResult> register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> register(RegisterInputModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

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
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    await _userManager.AddToRoleAsync(user, roleName);
                    _logger.LogInformation($"{model.FirstName} {model.LastName} created a new account with the Email address {model.Email}.");
                    return RedirectToAction("edit", "account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        // FORGOT PASSWORD : TODO
        public IActionResult forgot_password()
        {
            return View();
        }

        // EDIT ACCOUNT
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            ViewBag.UserEmail = user.Email;
            ViewBag.UserPasswordLength = user.Password.Length;

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View(new EditAccountViewModel
            {
                ChangeEmail = new ChangeEmailInputModel(),
                ChangePassword = new ChangePasswordInputModel()
            });
        }

        // ACCESS TO PROFILE, MEDICAL HISTORY, PRESCRIPTIONS
        [HttpGet]
        [Authorize]
        public IActionResult my_profile()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult my_medical_history()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult my_prescriptions()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
            return View();
        }

        // CHANGE EMAIL
        [HttpPost]
        public async Task<IActionResult> change_email(ChangeEmailInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            if (model.CurrentEmail == null || model.NewEmail == null || model.ConfirmNewEmail == null)
            {
                TempData["FailMessage"] = "All fields are required.";
                return RedirectToAction("edit");
            }

            if (model.CurrentEmail != User.Identity.Name)
            {
                TempData["FailMessage"] = "The current Email does not match your account Email.";
                return RedirectToAction("edit");
            }

            if (model.NewEmail == model.CurrentEmail || model.ConfirmNewEmail == model.CurrentEmail)
            {
                TempData["FailMessage"] = "The new Email must be different from the current Email.";
                return RedirectToAction("edit");
            }

            if (model.NewEmail != model.ConfirmNewEmail)
            {
                TempData["FailMessage"] = "The Emails do not match.";
                return RedirectToAction("edit");
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);

                if (existingUser != null && existingUser.Id != user.Id)
                {
                    TempData["FailMessage"] = "This Email is already in use.";
                    return RedirectToAction("edit");
                }

                user.Email = model.NewEmail;
                user.NormalizedEmail = model.NewEmail.ToUpper();
                user.UserName = model.NewEmail;
                user.NormalizedUserName = model.NewEmail.ToUpper();

                var emailResult = await _userManager.UpdateAsync(user);

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        _logger.LogError($"Email change error: {error.Description}");
                    }
                    return RedirectToAction("edit");
                }

                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your Email has been updated successfully.";
                return RedirectToAction("edit");
            }

            //TempData["FailMessage"] = "The Email change failed.";
            return RedirectToAction("edit");
        }

        // CHANGE PASSWORD : NEED FIXES 
        [HttpPost]
        public async Task<IActionResult> change_password(ChangePasswordInputModel model)
        {
            //_logger.LogInformation("*******************************************************************************************************\n************************************ Change Password attempt ******************************************\n*******************************************************************************************************");

            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            //_logger.LogInformation("*******************************************************************************************************\n************************************ Change Password tests ********************************************\n*******************************************************************************************************");

            if (model.CurrentPassword == null || model.NewPassword == null || model.ConfirmNewPassword == null)
            {
                //_logger.LogInformation("*******************************************************************************************************\n************************************ All fields required ********************************************\n*******************************************************************************************************");
                TempData["FailMessage"] = "All fields are required.";
                return RedirectToAction("edit");
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

            if (!passwordCheck)
            {
                TempData["FailMessage"] = "The current Password does not match your account Password.";
                return RedirectToAction("edit");
            }

            if (model.NewPassword == model.CurrentPassword || model.ConfirmNewPassword == model.CurrentPassword)
            {
                TempData["FailMessage"] = "The new Password must be different from the current Password.";
                return RedirectToAction("edit");
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                TempData["FailMessage"] = "The Passwords do not match.";
                return RedirectToAction("edit");
            }

            //_logger.LogInformation("*******************************************************************************************************\n********************************* Change Password is on its way ***************************************\n*******************************************************************************************************");

            user.Password = model.NewPassword;

            var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError("ChangePassword." + error.Code, error.Description);
                    _logger.LogError($"Password change error: {error.Description}");
                }
                return RedirectToAction("edit");
            }

            //_logger.LogInformation("*******************************************************************************************************\n************************************** Change Password done *******************************************\n*******************************************************************************************************");

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your Password has been updated successfully.";
            return RedirectToAction("edit");
        }

        // DELETE ACCOUNT BY KEEPING INFORMATIONS FOR MEDICAL HISTORY
        [HttpPost]
        public async Task<IActionResult> delete()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                user.Phone = "deleted";
                user.Address = "deleted";

                var newPassword = Guid.NewGuid().ToString();
                
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, newPassword);

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("index", "home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("edit");
                }
            }
            return RedirectToAction("index", "home");
        }

        // DELETE ACCOUNT BY DELETING ALL INFORMATIONS
        [HttpPost]
        public async Task<IActionResult> delete_all()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignOutAsync();
                        return RedirectToAction("index", "home");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("edit");
                    }
                }
            }
            return RedirectToAction("index", "home");
        }
    }
}