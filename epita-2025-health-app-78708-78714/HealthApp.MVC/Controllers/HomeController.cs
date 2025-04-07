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

        /*
         * This method is used to display the error page.
         */
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

        /*****************************************/

        /*
         * This method is used to display the home page.
         */
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

        /*
         * This method is used to display the home page.
         * seachInput: the input from the search bar
         * searchField: the field to search in
         */
        [HttpPost]
        public async Task<IActionResult> index([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            return RedirectToAction("doctors", "care", new { searchInput, searchField });
        }

        /*****************************************/

        /*
         * This method is used to display the login or register page.
         */
        public IActionResult login_or_register()
        {
            return View();
        }

        /*****************************************/

        /*
         * This method is used to display the user's messages page.
         */
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

        /*****************************************/

        /*
         * This method is used to display the user's appointments page.
         */
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

        /***************************************/

        /*
         * This method is used to display the user's account page.
         */
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

        /**************************************/

        /*
         * This method is used to log out the user.
         */
        [HttpPost]
        public async Task<IActionResult> logout()
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            _logger.LogInformation($"******************************\nUser {user.Email} has logged out.\n******************************\n");

            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        /************************************/

        /*
         * This method is used to display the cancellation policy page.
         */
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

        /************************************/

        /*
         * This method is used to display the contact page.
         * model: The model containing the message data.
         */
        public async Task<IActionResult> contact(ContactInputModel model)
        {
            // Get the current user
            var user = await _signInManager.UserManager.GetUserAsync(User);
            // Get the list of administrators
            var admins = await _signInManager.UserManager.GetUsersInRoleAsync("administrator");

            var isLogged = user != null;
            ViewBag.IsLogged = user != null;

            // Check if the user is logged in
            if (!isLogged)
            {
                // If not logged in, set the sender ID to a guest user ID
                user = await _signInManager.UserManager.FindByIdAsync("f27dde93-f22b-4a81-a322-4336fc5232f4");

                // Check if all the fields are filled
                if (ModelState.IsValid)
                {
                    // Send the message to all administrators
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
                            _logger.LogError($"Error sending message: {ex.Message}");
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
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return View(model);
                }
            }
            else
            {
                // If logged in, set the sender ID to the logged-in user's ID
                ViewBag.User = user;
                var userRoles = _signInManager.UserManager.GetRolesAsync(user).Result;
                ViewBag.IsLogged = user != null;
                ViewBag.IsDoctor = userRoles.Contains("doctor");
                ViewBag.IsPatient = userRoles.Contains("patient");
                ViewBag.IsAdmin = userRoles.Contains("administrator");

                // Check if all the fields are filled
                if (ModelState.IsValid)
                {
                    // Send the message to all administrators
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
                            TempData["ErrorMessage"] = "An error occurred while sending your message. Please try again later.";
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
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return View(model);
                }
            }
        }
    }
}