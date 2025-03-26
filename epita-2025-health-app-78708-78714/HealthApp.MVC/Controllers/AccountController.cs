#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using System.Data.Entity.Validation;
using System.Data.Entity;
using HealthApp.Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Globalization;

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
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

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
        public async Task<IActionResult> login()
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
                    return RedirectToAction("edit", "account");
                }
                else
                {
                    TempData["ErrorMessage"] = "Wrong credentials.\n";
                    return View(model);
                }
            }

            TempData["ErrorMessage"] = "Wrong credentials.\n";
            return View(model);
        }

        // REGISTER
        public async Task<IActionResult> register()
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
                var roleName = "patient";

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    await _userManager.AddToRoleAsync(user, roleName);
                    return RedirectToAction("edit", "account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                        return View(model);
                    }
                }
            }
            TempData["ErrorMessage"] = "Registration failed.\n";
            return View(model);
        }

        // FORGOT PASSWORD : TODO
        public IActionResult forgot_password()
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
            ViewBag.UserEmail = user.Email;
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
        public IActionResult my_profile()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");
            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            ViewBag.UserPhone = user.Phone;
            ViewBag.UserAddress = user.Address;

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> change_info(ChangeInfoInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                user.Phone = model.Phone;
                user.Address = model.Address;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your information has been updated.\n";
                return RedirectToAction("my_profile", "account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
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
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.CurrentEmail != User.Identity.Name)
            {
                TempData["ErrorMessage"] = "The current Email does not match your account Email.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewEmail == model.CurrentEmail || model.ConfirmNewEmail == model.CurrentEmail)
            {
                TempData["ErrorMessage"] = "The new Email must be different from the current Email.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewEmail != model.ConfirmNewEmail)
            {
                TempData["ErrorMessage"] = "The Emails do not match.\n";
                return RedirectToAction("edit", "account");
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);

                if (existingUser != null && existingUser.Id != user.Id)
                {
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
                    foreach (var error in emailResult.Errors)
                    {
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                    }
                    return RedirectToAction("edit", "account");
                }

                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your Email has been updated.\n";
                return RedirectToAction("edit", "account");
            }

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
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("edit", "account");
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

            if (!passwordCheck)
            {
                TempData["ErrorMessage"] = "The current Password does not match your account Password.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewPassword == model.CurrentPassword || model.ConfirmNewPassword == model.CurrentPassword)
            {
                TempData["ErrorMessage"] = "The new Password must be different from the current Password.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "The Passwords do not match.\n";
                return RedirectToAction("edit", "account");
            }

            user.Password = model.NewPassword;
            var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    TempData["ErrorMessage"] = $"{error.Description}\n";
                }
                return RedirectToAction("edit", "account");
            }

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
                            TempData["ErrorMessage"] = $"{error.Description}\n";
                        }
                        return View("edit", "account");
                    }
                }
            }
            return RedirectToAction("index", "home");
        }
    }
}