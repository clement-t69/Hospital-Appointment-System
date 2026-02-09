using HealthApp.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace HealthApp.MVC.Controllers
{
    public class LogsController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public LogsController(SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
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

        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleWebSocket(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task HandleWebSocket(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!webSocket.CloseStatus.HasValue)
            {
                string logMessage = $"Log: {DateTime.Now} - Some log message";
                byte[] bytes = Encoding.UTF8.GetBytes(logMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                await Task.Delay(1000);
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket closed", CancellationToken.None);
        }
    }
}