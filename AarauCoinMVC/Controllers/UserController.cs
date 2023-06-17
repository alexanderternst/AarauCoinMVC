using AarauCoinMVC.Models;
using AarauCoinMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AarauCoinMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IDatabaseCon _context;

        public UserController(ILogger<UserController> logger, IDatabaseCon context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Login()
        {
            _logger.LogInformation($"Login page says hello");
            return View();
        }

        public IActionResult LoginAction(LoginModel loginData)
        {
            try
            {
                var list = _context.GetUser(loginData);

                if (loginData.Username == list.Username && loginData.Password == list.Password)
                {
                    CreateLoginCookie(loginData, list.Level);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Handle failed login
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unknown Exception" + ex.Message);
                ViewBag.ErrorMessage = "Unknown Exception";
                ViewBag.ErrorType = "danger";
                return View("Login");
            }
        }

        public void CreateLoginCookie(LoginModel loginData, string level)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginData.Username), // Add claims as needed
                    new Claim (ClaimTypes.Role, level) // Add claims as needed
                };

            var claimsIdentity = new ClaimsIdentity(claims, "AarauCoin-AuthenticationScheme");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Set whether the cookie should persist across sessions
                ExpiresUtc = DateTime.UtcNow.AddMinutes(1) // Set the expiration time of the cookie
            };

            HttpContext.SignInAsync("AarauCoin-AuthenticationScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        public IActionResult LogoutAction(string username)
        {
            //var a = HttpContext.User.Claims.ToList();
            //var c = HttpContext.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Role);
            //var b = User.FindFirst(ClaimTypes.Role).Value.ToString();

            HttpContext.SignOutAsync("AarauCoin-AuthenticationScheme");
            return RedirectToAction("Index", "Home");
        }
    }
}