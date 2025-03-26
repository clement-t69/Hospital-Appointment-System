using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthApp.MVC.Models;
using HealthApp.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HealthApp.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(SignInManager<User> signInManager,
            ILogger<AccountController> logger, UserManager<User> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _context = context;
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
                    await _userManager.AddToRoleAsync(user, roleName);

                    TempData["SuccessMessage"] = $"New user with Email {model.Email} and role {roleName} has been created.\n";

                    return RedirectToAction("users", "admin");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                    }
                    return RedirectToAction("users", "admin");
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    TempData["ErrorMessage"] = $"{error.ErrorMessage}\n";
                }
                return RedirectToAction("users", "admin");
            }
        }

        public async Task<IActionResult> edit_user(EditUserInputModel model, string userId)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Email = model.Email;
                    user.Phone = model.Phone;
                    user.Address = model.Address;

                    var roleName = model.Role.ToString();

                    await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                    await _userManager.AddToRoleAsync(user, roleName);

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"User {user.Email} has been updated.\n";
                        return RedirectToAction("users", "admin");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            TempData["ErrorMessage"] = $"{error.Description}\n";
                        }
                        return RedirectToAction("users", "admin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"User with Id {userId} not found.\n";
                    return RedirectToAction("users", "admin");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("users", "admin");
            }
        }

        public async Task<IActionResult> delete_user(string userId)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    var admin = user.UserName == "admin@test.fr";
                    if (admin)
                    {
                        TempData["ErrorMessage"] = $"You cannot delete this admin user.\n";
                        return RedirectToAction("users", "admin");
                    }

                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = $"User has been deleted.\n";
                        return RedirectToAction("users", "admin");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            TempData["ErrorMessage"] = $"{error.Description}\n";
                        }
                        return RedirectToAction("users", "admin");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"User with Id {userId} not found.\n";
                    return RedirectToAction("users", "admin");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("users", "admin");
            }
        }
    }
}