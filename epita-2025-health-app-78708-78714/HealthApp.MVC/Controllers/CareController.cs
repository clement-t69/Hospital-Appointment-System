using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace HealthApp.MVC.Controllers
{
    public class CareController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public CareController(SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
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
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");
            return View();
        }

        public IActionResult center()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View();
        }

        public IActionResult patients()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user == null)
            {
                return View();
            }
            var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("Doctor");
            ViewBag.IsPatient = userRoles.Contains("Patient");
            ViewBag.IsAdmin = userRoles.Contains("Administrator");

            return View();
        }
    }
}