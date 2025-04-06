#nullable disable

using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HealthApp.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net;
using HealthApp.Domain.Data;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace HealthApp.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly SendEmailModel _sendEmailModel;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        private ChangeEmailInputModel changeEmailModel = new ChangeEmailInputModel();
        private ChangePasswordInputModel changePasswordModel = new ChangePasswordInputModel();

        public AccountController(SignInManager<User> signInManager,
            UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger, SendEmailModel sendEmailModel, IHttpContextAccessor httpContextAccessor
            , ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _sendEmailModel = sendEmailModel;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        // home/error.cshtml
        public IActionResult error()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            _logger.LogError($"******************************\nUser {user.Email} has encountered an error. (redirected to error page)\n******************************\n");
            return View();
        }


        // Shared/_Layout.cshtml
        // IF NOT LOGGED IN
        public IActionResult login_or_register()
        {
            return View();
        }

        // IF LOGGED IN
        public IActionResult my_notifications()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            ViewBag.userId = user.Id;

            var notifications = _context.Notifications
                .Where(n => n.ReceiverId == user.Id)
                .OrderByDescending(n => n.Date)
                .ToList();

            ViewBag.Notifications = notifications;

            return View();
        }

        public IActionResult mark_as_read(string userId)
        {
            var notifications = _context.Notifications
                .Where(n => n.ReceiverId == userId && n.IsRead == false)
                .OrderByDescending(n => n.Date)
                .ToList();

            if (notifications.Count == 0)
            {
                TempData["ErrorMessage"] = "No unread notifications found.";
                return RedirectToAction("my_notifications", "account");
            }
            else
            {
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }
            }

            _context.SaveChanges();
            return RedirectToAction("my_notifications", "account");
        }

        public IActionResult delete_notification(int id)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.Id == id);

            if (notification == null)
            {
                TempData["ErrorMessage"] = "Notification not found.";
                return RedirectToAction("my_notifications", "account");
            }
            else if (notification.Id == 0)
            {
                TempData["ErrorMessage"] = "You cannot delete this notification.";
                return RedirectToAction("my_notifications", "account");
            }
            else
            {
                _context.Notifications.Remove(notification);
                _context.SaveChanges();
            }

            return RedirectToAction("my_notifications", "account");
        }

        public IActionResult my_messages([FromQuery] string messagesSearchInput, 
            [FromQuery] string messagesSearchField, 
            [FromQuery] string sentMessagesSearchInput, 
            [FromQuery] string sentMessagesSearchField)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var userRoles = _userManager.GetRolesAsync(user).Result;
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            List<Message> messages = _context.Messages
                .Where(m => m.ReceiverId == user.Id)
                .OrderByDescending(m => m.Date)
                .ToList();

            List<Message> sentMessages = _context.Messages
                .Where(m => m.SenderId == user.Id)
                .OrderByDescending(m => m.Date)
                .ToList();

            if (messagesSearchInput == null || messagesSearchField == null)
            {
                messagesSearchInput = "";
                messagesSearchField = "";
            }
            if (sentMessagesSearchInput == null || sentMessagesSearchField == null)
            {
                sentMessagesSearchInput = "";
                sentMessagesSearchField = "";
            }

            if (messagesSearchInput != null || messagesSearchField != null)
            {
                messages = searchMessages(messagesSearchInput, messagesSearchField, user.Id);
            }

            if (sentMessagesSearchInput != null || sentMessagesSearchField != null)
            {
                sentMessages = searchSentMessages(sentMessagesSearchInput, sentMessagesSearchField, user.Id);
            }

            ViewBag.Messages = messages;
            ViewBag.SentMessages = sentMessages;

            return View();
        }

        public List<Message> searchMessages(string searchInput, string searchField, string receiverId)
        {
            var messages = _context.Messages.ToList().Where(m => m.ReceiverId == receiverId).ToList();

            if (searchField == "Date")
            {
                messages = messages.Where(m => m.Date.ToLower().Contains(searchInput) && m.ReceiverId == receiverId).ToList();
            }
            else if (searchField == "Sender")
            {
                messages = messages.Where(m => m.SenderFirstName.ToLower().Contains(searchInput) || m.SenderLastName.ToLower().Contains(searchInput) && m.ReceiverId == receiverId).ToList();
            }
            else if (searchField == "Object")
            {
                messages = messages.Where(m => m.Object.ToLower().Contains(searchInput) && m.ReceiverId == receiverId).ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return messages;
        }

        public List<Message> searchSentMessages(string searchInput, string searchField, string senderId)
        {
            var messages = _context.Messages.ToList().Where(m => m.SenderId == senderId).ToList();

            if (searchField == "Date")
            {
                messages = messages.Where(m => m.Date.ToLower().Contains(searchInput) && m.SenderId == senderId).ToList();
            }
            else if (searchField == "Receiver")
            {
                messages = messages.Where(m => m.ReceiverFirstName.ToLower().Contains(searchInput) || m.ReceiverLastName.ToLower().Contains(searchInput) && m.SenderId == senderId).ToList();
            }
            else if (searchField == "Object")
            {
                messages = messages.Where(m => m.Object.ToLower().Contains(searchInput) && m.SenderId == senderId).ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return messages;
        }

        public async Task<IActionResult> message(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");
            ViewBag.UserId = user.Id;

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login_or_register", "account");
            }

            var message = _context.Messages.FirstOrDefault(m => m.Id == id);

            if (message == null)
            {
                TempData["ErrorMessage"] = "Message not found.";
                return RedirectToAction("my_messages", "account");
            }

            if (message.IsRead == false && message.ReceiverId == user.Id)
            {
                message.IsRead = true;
                _context.SaveChanges();
            }

            ViewBag.Message = message;

            return View(message);
        }

        public async Task<IActionResult> send_message()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            ViewBag.User = user;

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login", "account");
            }

            ViewBag.Doctors = _context.Doctors.ToList();
            ViewBag.Patients = _context.Patients.ToList();

            if (userRoles.Contains("doctor"))
            {
                ViewBag.Receiver = _userManager.GetUsersInRoleAsync("patient").Result;
            }
            else if (userRoles.Contains("patient"))
            {
                ViewBag.Receiver = _userManager.GetUsersInRoleAsync("doctor").Result;
            }

            return View();
        }
        
        public async Task<IActionResult> new_message(NewMessageInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login", "account");
            }

            ViewBag.SenderId = user.Id;

            _logger.LogInformation($"******************************\nSenderId: {model.SenderId}\nReceiverId: {model.ReceiverId}\nObject: {model.Object}\nMessage: {model.Message}\n******************************\n");

            var sender = await _userManager.FindByIdAsync(model.SenderId);
            var receiver = await _userManager.FindByIdAsync(model.ReceiverId);

            if (sender == null || receiver == null)
            {
                TempData["ErrorMessage"] = "Sender or receiver not found.";
                return RedirectToAction("send_message", "account");
            }

            var id = _context.Messages.Max(m => m.Id) + 1;

            if (ModelState.IsValid)
            {
                if (userRoles.Contains("doctor"))
                {
                    var doctorId = user.Id;
                    var patientId = model.ReceiverId;

                    var message = new Message
                    {
                        Id = id,
                        DoctorId = doctorId,
                        PatientId = patientId,
                        SenderId = model.SenderId,
                        SenderFirstName = user.FirstName,
                        SenderLastName = user.LastName,
                        ReceiverId = model.ReceiverId,
                        ReceiverFirstName = receiver.FirstName,
                        ReceiverLastName = receiver.LastName,
                        Object = model.Object,
                        Content = model.Message,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        Type = "New",
                        IsRead = false
                    };

                    var notification = new Notification
                    {
                        Id = _context.Notifications.Max(n => n.Id) + 1,
                        SenderId = user.Id,
                        ReceiverId = model.ReceiverId,
                        Content = "You have received a new message from " + user.FirstName + " " + user.LastName,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        IsRead = false
                    };

                    try
                    {
                        _context.Messages.Add(message);
                        _context.Notifications.Add(notification);
                        _context.SaveChanges();
                        TempData["SuccessMessage"] = "Message sent successfully.";
                        _logger.LogInformation($"******************************\nDoctor {user.UserName} has sent a message to {receiver.UserName}.\n******************************\n");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                        TempData["ErrorMessage"] = "An error occurred while sending the message.";
                    }
                }
                else if (userRoles.Contains("patient"))
                {
                    var doctorId = model.ReceiverId;
                    var patientId = user.Id;

                    var message = new Message
                    {
                        Id = id,
                        DoctorId = doctorId,
                        PatientId = patientId,
                        SenderId = model.SenderId,
                        SenderFirstName = user.FirstName,
                        SenderLastName = user.LastName,
                        ReceiverId = model.ReceiverId,
                        ReceiverFirstName = receiver.FirstName,
                        ReceiverLastName = receiver.LastName,
                        Object = model.Object,
                        Content = model.Message,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        Type = "New",
                        IsRead = false
                    };

                    var notification = new Notification
                    {
                        Id = _context.Notifications.Max(n => n.Id) + 1,
                        SenderId = user.Id,
                        ReceiverId = model.ReceiverId,
                        Content = "You have received a new message from " + user.FirstName + " " + user.LastName,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        IsRead = false
                    };

                    try
                    {
                        _context.Messages.Add(message);
                        _context.Notifications.Add(notification);
                        _context.SaveChanges();
                        TempData["SuccessMessage"] = "Message sent successfully.";
                        _logger.LogInformation($"******************************\nPatient {user.UserName} has sent a message to {receiver.UserName}.\n******************************\n");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                        TempData["ErrorMessage"] = "An error occurred while sending the message.";
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid input.";
            }
            return RedirectToAction("my_messages", "account");
        }

        public async Task<IActionResult> reply_message(ReplyMessageInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("login", "account");
            }

            ViewBag.SenderId = user.Id;

            var sender = await _userManager.FindByIdAsync(model.replySenderId);
            var receiver = await _userManager.FindByIdAsync(model.replyReceiverId);

            if (sender == null || receiver == null)
            {
                TempData["ErrorMessage"] = "Sender or receiver not found.";
                return RedirectToAction("my_messages", "account");
            }

            if (model.replyMessage == null || model.replyObject == null || model.replyReceiverId == null || model.replySenderId == null)
            {
                TempData["ErrorMessage"] = "Your message cannot be empty.";
                return RedirectToAction("message", "account", new { Id = model.Id });
            }

            var id = _context.Messages.Max(m => m.Id) + 1;

            if (ModelState.IsValid)
            {
                if (userRoles.Contains("doctor"))
                {
                    var doctorId = user.Id;
                    var patientId = model.replyReceiverId;

                    var message = new Message
                    {
                        Id = id,
                        DoctorId = doctorId,
                        PatientId = patientId,
                        SenderId = model.replySenderId,
                        SenderFirstName = user.FirstName,
                        SenderLastName = user.LastName,
                        ReceiverId = model.replyReceiverId,
                        ReceiverFirstName = receiver.FirstName,
                        ReceiverLastName = receiver.LastName,
                        Object = "RE: " + model.replyObject,
                        Content = model.replyMessage,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        Type = "Reply",
                        IsRead = false
                    };

                    var notification = new Notification
                    {
                        Id = _context.Notifications.Max(n => n.Id) + 1,
                        SenderId = user.Id,
                        ReceiverId = model.replyReceiverId,
                        Content = "You have received a new message from " + user.FirstName + " " + user.LastName,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        IsRead = false
                    };

                    try
                    {
                        _context.Messages.Add(message);
                        _context.Notifications.Add(notification);
                        _context.SaveChanges();
                        TempData["SuccessMessage"] = "Message sent successfully.";
                        _logger.LogInformation($"******************************\nDoctor {user.UserName} has sent a message to {receiver.UserName}.\n******************************\n");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                        TempData["ErrorMessage"] = "An error occurred while sending the message.";
                    }
                }
                else if (userRoles.Contains("patient"))
                {
                    var doctorId = model.replyReceiverId;
                    var patientId = user.Id;

                    var message = new Message
                    {
                        Id = id,
                        DoctorId = doctorId,
                        PatientId = patientId,
                        SenderId = model.replySenderId,
                        SenderFirstName = user.FirstName,
                        SenderLastName = user.LastName,
                        ReceiverId = model.replyReceiverId,
                        ReceiverFirstName = receiver.FirstName,
                        ReceiverLastName = receiver.LastName,
                        Object = "RE: " + model.replyObject,
                        Content = model.replyMessage,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        Type = "Reply",
                        IsRead = false
                    };

                    var notification = new Notification
                    {
                        Id = _context.Notifications.Max(n => n.Id) + 1,
                        SenderId = user.Id,
                        ReceiverId = model.replyReceiverId,
                        Content = "You have received a new message from " + user.FirstName + " " + user.LastName,
                        Date = DateTime.Now.ToString("yyyy-MM-dd - hh:mm tt"),
                        IsRead = false
                    };

                    try
                    {
                        _context.Messages.Add(message);
                        _context.Notifications.Add(notification);
                        _context.SaveChanges();
                        TempData["SuccessMessage"] = "Message sent successfully.";
                        _logger.LogInformation($"******************************\nPatient {user.UserName} has sent a message to {receiver.UserName}.\n******************************\n");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                        TempData["ErrorMessage"] = "An error occurred while sending the message.";
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid input.";
            }
            return RedirectToAction("message", "account", new {Id = id});
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
            User user = await _userManager.GetUserAsync(User);
            _logger.LogInformation($"******************************\nUser {user.Email} has logged out.\n******************************\n");

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


        // account/login.cshtml
        public IActionResult login()
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
                    _logger.LogInformation($"******************************\nUser {model.Email} has logged in.\n******************************\n");
                    return RedirectToAction("edit", "account");
                }
                else
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to log in.\n******************************\n");
                    TempData["ErrorMessage"] = "Wrong credentials.\n";
                    return View(model);
                }
            }

            _logger.LogError($"******************************\nUser {model.Email} has failed to log in.\n******************************\n");
            TempData["ErrorMessage"] = "Wrong credentials.\n";
            return View(model);
        }


        // account/register.cshtml
        public IActionResult register()
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
                var roleName = "Patient";

                if (result.Succeeded)
                {
                    _logger.LogInformation($"******************************\nUser {model.Email} has registered.\n******************************\n");

                    _sendEmailModel.SendCreateConfirmation(model.FirstName, model.LastName, model.Email, "user");

                    await _signInManager.SignInAsync(user, isPersistent: true);
                    await _userManager.AddToRoleAsync(user, roleName);

                    try
                    {
                        var patient = new Patient
                        {
                            UserId = user.Id
                        };
                        patient.Appointments = new List<Appointment>();
                        patient.MedicalHistories = new List<MedicalHistory>();
                        patient.Notifications = new List<Message>();
                        patient.Prescriptions = new List<Prescription>();
                        patient.FirstName = user.FirstName;
                        patient.LastName = user.LastName;

                        _context.Patients.Add(patient);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                    }

                    return RedirectToAction("edit", "account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"******************************\nUser {model.Email} has failed to register.\n******************************\n");
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                        return View(model);
                    }
                }
            }
            _logger.LogError($"******************************\nUser {model.Email} has failed to register.\n******************************\n");
            TempData["ErrorMessage"] = "Registration failed.\n";
            return View(model);
        }


        // account/forgot_password.cshtml
        public IActionResult forgot_password()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> forgot_password(ForgotPasswordInputModel model)
        {
            if (ModelState.IsValid)
            {
                string emailToLower = model.Email.ToLower();

                var user = await _userManager.FindByEmailAsync(emailToLower);
                if (user == null)
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password: Email not found.\n******************************\n");
                    TempData["ErrorMessage"] = "Email not found.\n";
                    return View(model);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var existingToken = _context.UserTokens.FirstOrDefault(t => t.UserId == user.Id && t.LoginProvider == "Default" && t.Name == "PasswordReset");

                if (existingToken == null)
                {
                    //_logger.LogError($"******************************\nToken {token} has been generated for User {model.Email}.\n******************************\n");
                    try
                    {
                        _context.UserTokens.Add(new IdentityUserToken<string>
                        {
                            UserId = user.Id,
                            LoginProvider = "Default",
                            Name = "PasswordReset",
                            Value = token
                        });

                        //_logger.LogInformation($"******************************\nToken {token} has been inserted manually for User {model.Email}.\n******************************\n");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                    }
                }
                else
                    existingToken.Value = token;

                await _context.SaveChangesAsync();

                var callbackUrl = Url.Action(
                    "reset_password",
                    "account",
                    new { token = WebUtility.UrlEncode(token), email = user.UserName },
                    //new { token, email = user.UserName },
                    Request.Scheme
                );


                _sendEmailModel.SendForgotPassword(user.FirstName, user.LastName, model.Email, token, callbackUrl);

                _logger.LogInformation($"******************************\nUser {model.Email} has requested a Password reset.\n******************************\n");
                TempData["SuccessMessage"] = "An Email has been sent to reset your Password.\n";
                
                return RedirectToAction("login", "account");
            }
            
            _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password.\n******************************\n");
            TempData["ErrorMessage"] = "An error occurred while resetting your Password.\n";
            return View(model);
        }

        public IActionResult reset_password(string token, string email)
        {
            ViewBag.token = WebUtility.UrlDecode(token);
            ViewBag.email = email;

            //_logger.LogInformation($"Recevied Email (GET) : {email}");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> reset_password(ResetPasswordInputModel model)
        {
            //_logger.LogInformation($"Received Email (POST) : {model.Email}");

            if (ModelState.IsValid)
            {
                var decodedToken = WebUtility.UrlDecode(model.Token);
                //var decodedToken = model.Token;

                _logger.LogInformation($"******************************\nEmail: {model.Email}\n******************************\n");

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password: Email not found.\n******************************\n");
                    TempData["ErrorMessage"] = "Email not found.\n";
                    return RedirectToAction("forgot_password", "account");
                }

                _logger.LogInformation($"******************************\nEmail: {user.UserName}\nPassword: {model.NewPassword}\n******************************\n");
                
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"******************************\nUser {model.Email} has reset their Password.\n******************************\n");
                    TempData["SuccessMessage"] = "Your Password has been reset.\n";

                    _context.UserTokens.RemoveRange(_context.UserTokens.Where(t => t.UserId == user.Id));
                    await _context.SaveChangesAsync();

                    return RedirectToAction("password_changed", "account");
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogError($"******************************\nUser {model.Email} has failed to reset their Password: {error.Description}\n******************************\n");
                    TempData["ErrorMessage"] = $"{error.Description}\n";
                }
            }
            return View(model);
        }

        public IActionResult password_changed()
        {
            return View();
        }


        // account/edit.cshtml
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
            ViewBag.UserEmail = user.UserName;
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

        // account/my_profile.cshtml
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> my_profile()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            ViewBag.UserFirstName = user.FirstName;
            ViewBag.UserLastName = user.LastName;
            ViewBag.Phone = user.Phone;
            ViewBag.Address = user.Address;

            if ((await _userManager.GetRolesAsync(user)).Contains("doctor") || (await _userManager.GetRolesAsync(user)).Contains("Doctor"))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);

                if (doctor != null)
                {
                    ViewBag.DoctorSpecialization = doctor.Specialization;
                    ViewBag.DoctorLocation = doctor.Location;
                }
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> change_info(ChangeInfoInputModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);

            if (patient != null)
            {
                user.Phone = model.Phone;
                user.Address = model.Address;
            }
            else if (doctor != null)
            {
                user.Phone = model.Phone;
                user.Address = model.Address;

                if (string.IsNullOrEmpty(model.Location) || string.IsNullOrEmpty(model.Specialization))
                {
                    TempData["ErrorMessage"] = "Location and Specialization cannot be empty.";
                    return RedirectToAction("my_profile", "account");
                }

                doctor.Specialization = model.Specialization;
                doctor.Location = model.Location;
            }         

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _context.SaveChanges();
                _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, user.UserName, "user", "information");

                _logger.LogInformation($"******************************\nUser {user.Email} has updated their information.\n******************************\n");
                TempData["SuccessMessage"] = "Your information has been updated.\n";
                return RedirectToAction("my_profile", "account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"******************************\nUser {user.Email} has failed to update their information.\n******************************\n");
                    TempData["ErrorMessage"] = $"{error.Description}\n";
                }
            }
            return RedirectToAction("my_profile", "account");
        }

        // account/my_medical_history.cshtml
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> my_medical_history([FromQuery] string searchInput, [FromQuery] string searchField)
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.IsLogged = user != null;
            ViewBag.IsDoctor = userRoles.Contains("doctor");
            ViewBag.IsPatient = userRoles.Contains("patient");
            ViewBag.IsAdmin = userRoles.Contains("administrator");

            if (searchInput == null || searchField == null)
            {
                searchInput = "";
                searchField = "";
            }

            var medicalHistories = searchMedicalHistory(searchInput, searchField, user.Id);

            if (searchInput != "" && searchField != "" && medicalHistories.Count == 0)
            {
                TempData["ErrorMessage"] = "No medical history found.";
                _logger.LogError($"******************************\nNo medical history found while searching for medical histories with {searchField} containing {searchInput}.\n******************************\n");
            }

            _logger.BeginScope($"******************************\nUser {user.UserName} has searched for medical history with {searchField} containing {searchInput}.\n******************************\n");

            ViewBag.MedicalHistories = medicalHistories;

            return View();
        }

        public List<MedicalHistory> searchMedicalHistory(string searchInput, string searchField, string patientId)
        {
            var medicalHistories = _context.MedicalHistories.ToList().Where(mh => mh.PatientId == patientId).ToList();

            if (searchField == "Date")
            {
                medicalHistories = medicalHistories.Where(mh => mh.Date.ToLower().Contains(searchInput) && mh.PatientId == patientId).ToList();
            }
            else if (searchField == "Doctor")
            {
                medicalHistories = medicalHistories.Where(mh => mh.DoctorFirstName.ToLower().Contains(searchInput) || mh.DoctorLastName.ToLower().Contains(searchInput) && mh.PatientId == patientId).ToList();
            }
            else if (searchField == "Speciality")
            {
                medicalHistories = medicalHistories.Where(mh => mh.Specialization.ToLower().Contains(searchInput) && mh.PatientId == patientId).ToList();
            }
            else if (searchField == "Location")
            {
                medicalHistories = medicalHistories.Where(mh => mh.Location.ToLower().Contains(searchInput) && mh.PatientId == patientId).ToList();
            }
            else
            {
                searchInput = "";
                searchField = "";
            }

            return medicalHistories;
        }

        // account/my_prescriptions.cshtml
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

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
            if (patient != null)
            {
                var prescriptions = _context.Prescriptions.Where(p => p.PatientId == patient.UserId).ToList();
                ViewBag.Prescriptions = prescriptions;
            }

            return View();
        }

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
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: All fields are required.\n******************************\n");
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.CurrentEmail != User.Identity.Name)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: The current Email does not match their account Email.\n******************************\n");
                TempData["ErrorMessage"] = "The current Email does not match your account Email.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewEmail == model.CurrentEmail || model.ConfirmNewEmail == model.CurrentEmail)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: The new Email must be different from the current Email.\n******************************\n");
                TempData["ErrorMessage"] = "The new Email must be different from the current Email.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewEmail != model.ConfirmNewEmail)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: The Emails do not match.\n******************************\n");
                TempData["ErrorMessage"] = "The Emails do not match.\n";
                return RedirectToAction("edit", "account");
            }

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.NewEmail);

                if (existingUser != null && existingUser.Id != user.Id)
                {
                    _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email: This Email is already in use.\n******************************\n");
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
                    _logger.LogError($"******************************\n");
                    foreach (var error in emailResult.Errors)
                    {
                        _logger.LogError($"User {user.Email} has failed to update their Email: {error.Description}\n");
                        TempData["ErrorMessage"] = $"{error.Description}\n";
                    }
                    _logger.LogError($"******************************\n");
                    return RedirectToAction("edit", "account");
                }

                _logger.LogInformation($"******************************\nUser {user.Email} has updated their Email to {model.NewEmail}.\n******************************\n");

                _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, model.CurrentEmail, "user", "email");

                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your Email has been updated.\n";
                return RedirectToAction("edit", "account");
            }

            _logger.LogError($"******************************\nUser {user.Email} has failed to update their Email.\n******************************\n");
            TempData["ErrorMessage"] = "An error occurred while updating your Email.\n";
            return RedirectToAction("edit", "account");
        }

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
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: All fields are required.\n******************************\n");
                TempData["ErrorMessage"] = "All fields are required.\n";
                return RedirectToAction("edit", "account");
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

            if (!passwordCheck)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: The current Password does not match their account Password.\n******************************\n");
                TempData["ErrorMessage"] = "The current Password does not match your account Password.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewPassword == model.CurrentPassword || model.ConfirmNewPassword == model.CurrentPassword)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: The new Password must be different from the current Password.\n******************************\n");
                TempData["ErrorMessage"] = "The new Password must be different from the current Password.\n";
                return RedirectToAction("edit", "account");
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                _logger.LogError($"******************************\nUser {user.Email} has failed to update their Password: The Passwords do not match.\n******************************\n");
                TempData["ErrorMessage"] = "The Passwords do not match.\n";
                return RedirectToAction("edit", "account");
            }

            user.Password = model.NewPassword;
            var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!passwordResult.Succeeded)
            {
                _logger.LogError($"******************************\n");
                foreach (var error in passwordResult.Errors)
                {
                    _logger.LogError($"User {user.Email} has failed to update their Password: {error.Description}\n");
                    TempData["ErrorMessage"] = $"{error.Description}\n";
                }
                _logger.LogError($"******************************\n");
                return RedirectToAction("edit", "account");
            }

            _logger.LogInformation($"******************************\nUser {user.Email} has updated their Password.\n******************************\n");

            _sendEmailModel.SendEditConfirmation(user.FirstName, user.LastName, user.UserName, "user", "password");

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your Password has been updated.\n";
            return RedirectToAction("edit", "account");
        }

        [HttpPost]
        public async Task<IActionResult> delete_all()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userRoles = await _userManager.GetRolesAsync(user);

                if (user != null)
                {
                    var userEmail = user.UserName;
                    var userFirstName = user.FirstName;
                    var userLastName = user.LastName;

                    if (userEmail == "admin@test.fr")
                    {
                        _logger.LogError($"******************************\nUser {userEmail} has failed to delete their account: Admin account cannot be deleted.\n******************************\n");
                        TempData["ErrorMessage"] = "Admin account cannot be deleted.\n";
                        return RedirectToAction("edit", "account");
                    }

                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        try
                        {
                            if (userRoles.Contains("patient"))
                            {
                                var patient = _context.Patients.FirstOrDefault(p => p.UserId == user.Id);
                                if (patient != null)
                                {
                                    _context.Patients.Remove(patient);
                                    _context.SaveChanges();
                                }

                                _logger.LogInformation($"******************************\nUser {userEmail} has deleted their account.\n******************************\n");
                            }
                            else if (userRoles.Contains("doctor"))
                            {
                                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                                if (doctor != null)
                                {
                                    _context.Doctors.Remove(doctor);
                                    _context.SaveChanges();
                                }
                                _logger.LogInformation($"******************************\nUser {userEmail} has deleted their account.\n******************************\n");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"******************************\n{ex.Message}\n******************************\n");
                        }

                        _sendEmailModel.SendDeleteConfirmation(userFirstName, userLastName, userEmail, "user");

                        await _signInManager.SignOutAsync();
                        return RedirectToAction("index", "home");
                    }
                    else
                    {
                        _logger.LogError($"******************************\n");
                        foreach (var error in result.Errors)
                        {
                            _logger.LogError($"User {userEmail} has failed to delete their account: {error.Description}\n");
                            TempData["ErrorMessage"] = $"{error.Description}\n";
                        }
                        _logger.LogError($"******************************\n");
                        return View("edit", "account");
                    }
                }
            }
            return RedirectToAction("index", "home");
        }
    }
}