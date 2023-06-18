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

        public IActionResult LoginAction(LoginViewModel loginData)
        {
            try
            {
                if (loginData.Username == null || loginData.Password == null)
                    throw new Exception("Username or password is null");

                var user = _context.GetUser(loginData.Username);

                if (user == null)
                    throw new LoginFailedException();

                if (user.Coins == null)
                    TempData["Coins"] = "No coins registered";
                else
                    TempData["Coins"] = user.Coins.Coins.ToString();

                if (loginData.Username.ToLower() == user.Username.ToLower() && loginData.Password == user.Password)
                {
                    CreateLoginCookie(loginData, user.Level);
                    _logger.LogInformation($"User {loginData.Username} logged in");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    throw new LoginFailedException();
                }
            }
            catch (LoginFailedException)
            {
                _logger.LogInformation($"User with {loginData.Username} failed to log in");
                ViewBag.ErrorMessage = "Login failed, incorrect password or username";
                ViewBag.ErrorType = "info";
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unknown Exception" + ex.Message);
                ViewBag.ErrorMessage = "Unknown Exception";
                ViewBag.ErrorType = "danger";
                return View("Login");
            }
        }

        private void CreateLoginCookie(LoginViewModel loginData, string level)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginData.Username),
                    new Claim (ClaimTypes.Role, level)
                };

            var claimsIdentity = new ClaimsIdentity(claims, "AarauCoin-AuthenticationScheme");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(1)
            };

            HttpContext.SignInAsync("AarauCoin-AuthenticationScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        public IActionResult Logout()
        {
            //var a = HttpContext.User.Claims.ToList();
            //var c = HttpContext.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Role);
            //var b = User.FindFirst(ClaimTypes.Role).Value.ToString();

            HttpContext.SignOutAsync("AarauCoin-AuthenticationScheme");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Account()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userInformation = _context.GetUserInformation(User.Identity.Name);
                _logger.LogInformation($"User {User.Identity.Name} loaded account page");
                return View(userInformation);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
    }
}