#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.Emit;

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

        // HEADER
        // IF NOT LOGGED IN
        public IActionResult login_or_register()
        {
            return View();
        }

        // IF LOGGED IN
        public IActionResult my_messages()
        {
            return View();
        }
        public IActionResult my_appointments()
        {
            return View();
        }
        public IActionResult edit()
        {
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
            ViewBag.IsDoctor = userRoles.Contains("Doctor");

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
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult my_medical_history()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult my_prescriptions()
        {
            return View();
        }

        // CHANGE EMAIL
        [HttpPost]
        public async Task<IActionResult> change_email(ChangeEmailInputModel model)
        {
            if (model.CurrentEmail != User.Identity.Name)
            {
                ModelState.AddModelError("CurrentEmail", "The Current Email does not match your Account Email.");
                ViewData.ModelState.AddModelError("ChangeEmail", "");
                return View("edit", new EditAccountViewModel { ChangeEmail = model });
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);

                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("NewEmail", "This email is already in use.");
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

            ViewData.ModelState.AddModelError("ChangeEmail", "");
            return View("edit", new EditAccountViewModel { ChangeEmail = model });
        }

        // CHANGE PASSWORD
        [HttpPost]
        public async Task<IActionResult> change_password(ChangePasswordInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

            if (!passwordCheck)
            {
                ModelState.AddModelError("ChangePassword.CurrentPassword", "The Current Password does not match your Account Password.");
                ViewData.ModelState.AddModelError("ChangePassword", "");
                return View("edit", new EditAccountViewModel { ChangePassword = model });
            }

            if (ModelState.IsValid)
            { 
                _logger.LogInformation("Password change attempt");
                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("ChangePassword." + error.Code, error.Description);
                        _logger.LogError($"Password change error: {error.Description}");
                    }
                    ViewData.ModelState.AddModelError("ChangePassword", "");
                    return View("edit", new EditAccountViewModel { ChangePassword = model });
                }

                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your Password has been updated successfully.";
                return RedirectToAction("edit");
            }

            ViewData.ModelState.AddModelError("ChangePassword", "");
            return View("edit", new EditAccountViewModel { ChangePassword = model });
        }

        // DELETE ACCOUNT
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
    }
}