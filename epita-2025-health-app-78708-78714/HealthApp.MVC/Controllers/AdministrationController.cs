using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthApp.MVC.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AdministrationController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
    } 
}
