using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AdminController(SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult panel()
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

        public IActionResult appointments()
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

        public IActionResult users()
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