using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public HomeController(SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult index()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
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
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View();
        }
        public IActionResult my_appointments()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View();
        }
        public IActionResult edit()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
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
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View();
        }

        public IActionResult contact()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View();
        }
    }
}