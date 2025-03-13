#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HealthApp.MVC.Models;
using System.Collections.Generic;
using System.Linq;

namespace HealthApp.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

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

        // FORGOT PASSWORD
        public async Task<IActionResult> forgot_password(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();

        }
    }
}