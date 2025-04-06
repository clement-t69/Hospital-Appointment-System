using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HealthApp.MVC.Models;
using AspNetCoreGeneratedDocument;
using HealthApp.Domain.Data;

namespace HealthApp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(SignInManager<User> signInManager,
            ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
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

            _logger.LogError($"******************************\nUser {user.Email} has encountered an error. (redirected to error page)\n******************************\n");
            return View();
        }

        public IActionResult index()
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

        [HttpPost]
        public async Task<IActionResult> index([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            return RedirectToAction("doctors", "care", new { searchInput = searchInput, searchField = searchField });
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
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");
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
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            return RedirectToAction("appointments", "care");
        }
        
        public IActionResult edit()
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
        
        [HttpPost]
        public async Task<IActionResult> logout()
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            _logger.LogInformation($"******************************\nUser {user.Email} has logged out.\n******************************\n");

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
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");
            return View();
        }
    
        public async Task<IActionResult> contact(ContactInputModel model)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            var admins = await _signInManager.UserManager.GetUsersInRoleAsync("administrator");

            var isLogged = user != null;
            ViewBag.IsLogged = user != null;

            if (!isLogged)
            {
                user = await _signInManager.UserManager.FindByIdAsync("f27dde93-f22b-4a81-a322-4336fc5232f4");
                
                if (ModelState.IsValid)
                {
                    foreach (var admin in admins)
                    {
                        var adminId = admin.Id;

                        var message = new Message
                        {
                            Id = _context.Messages.Max(m => m.Id) + 1,
                            SenderId = model.SenderId,
                            SenderFirstName = user.FirstName,
                            SenderLastName = user.LastName,
                            ReceiverId = adminId,
                            ReceiverFirstName = admin.FirstName,
                            ReceiverLastName = admin.LastName,
                            DoctorId = "79b4a630-45a4-45f2-8fa6-af550e965cce",
                            PatientId = model.SenderId,
                            Object = $"{model.Email}: {model.Object}",
                            Content = model.Message,
                            Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                            Type = "New"
                        };

                        try
                        {
                            await _context.Messages.AddAsync(message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error adding message: {ex.Message}");
                            ModelState.AddModelError("", "An error occurred while sending your message. Please try again later.");
                            return View(model);
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"******************************\nGuest user {model.Email} has sent a message to the administrator.\n******************************\n");
                    TempData["SuccessMessage"] = "Your message has been sent successfully. We will get back to you as soon as possible.";
                    return RedirectToAction("contact", "home");
                }
                else
                {
                    ModelState.AddModelError("", "Please fill in all required fields.");
                    return View(model);
                }
            }
            else
            {
                ViewBag.User = user;

                if (ModelState.IsValid)
                {
                    foreach (var admin in admins)
                    {
                        var adminId = admin.Id;

                        var message = new Message
                        {
                            Id = _context.Messages.Max(m => m.Id) + 1,
                            SenderId = model.SenderId,
                            SenderFirstName = user.FirstName,
                            SenderLastName = user.LastName,
                            ReceiverId = adminId,
                            ReceiverFirstName = admin.FirstName,
                            ReceiverLastName = admin.LastName,
                            DoctorId = "79b4a630-45a4-45f2-8fa6-af550e965cce",
                            PatientId = model.SenderId,
                            Object = $"{model.Object}",
                            Content = model.Message,
                            Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                            Type = "New"
                        };

                        try
                        {
                            await _context.Messages.AddAsync(message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error adding message: {ex.Message}");
                            ModelState.AddModelError("", "An error occurred while sending your message. Please try again later.");
                            return View(model);
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"******************************\nGuest user {model.Email} has sent a message to the administrator.\n******************************\n");
                    TempData["SuccessMessage"] = "Your message has been sent successfully. We will get back to you as soon as possible.";
                    return RedirectToAction("contact", "home");
                }
                else
                {
                    ModelState.AddModelError("", "Please fill in all required fields.");
                    return View(model);
                }
            }
        }
    }
}