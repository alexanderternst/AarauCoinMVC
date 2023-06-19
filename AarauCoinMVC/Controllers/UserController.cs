using AarauCoinMVC.Models;
using AarauCoinMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
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
                if (loginData == null)
                    throw new Exception("Username or password is null");
                if (loginData.Username == null || loginData.Password == null)
                    throw new Exception("Username or password is null");

                var user = _context.GetUser(loginData.Username);

                if (user == null)
                    throw new LoginFailedException();

                //if (user.Coins == null)
                //    TempData["Coins"] = "No coins registered";
                //else
                //    TempData["Coins"] = user.Coins.Coins.ToString();

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
            _logger.LogInformation($"Authentification cookie for user {loginData.Username} was created");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync("AarauCoin-AuthenticationScheme");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Account()
        {
            if (User.Identity.IsAuthenticated)
            {
                try 
                {
                    AccountViewModel? userInformation = _context.GetUserInformation(User.Identity.Name);
                    
                    List<string> users = _context.GetUserNames();
                    if (users != null)
                    {
                        users.Remove(User.Identity.Name.ToString());
                        userInformation.Users = users.ToArray();
                    }

                    _logger.LogInformation($"User {User.Identity.Name} loaded account page");

                    if (User.IsInRole("Admin"))
                    {
                        return View("AdminAccount", userInformation);
                    }
                    else
                    {
                        return View("Account", userInformation);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    ViewBag.ErrorMessage = "Unknown Exception";
                    ViewBag.ErrorType = "danger";
                    return RedirectToAction("Account", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public IActionResult SendMoney(string reciever, int amount)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    _context.SendMoney(User.Identity.Name, reciever, amount);
                    _logger.LogInformation($"User {User.Identity.Name} sent {amount} coins to {reciever}");
                    return RedirectToAction("Account", "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    ViewBag.ErrorMessage = "Unknown Exception";
                    ViewBag.ErrorType = "danger";
                    // return View wont work because i need user data again, but cant use redirect because i need to show error message
                    return RedirectToAction("Account", "User");
                    //return View("Account");

                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }

        }

        public IActionResult CreateUser(string username, string password, string level, double coins)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                try
                {
                    _context.CreateUser(username, password, level, coins);
                    return RedirectToAction("Account", "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    ViewBag.ErrorMessage = "Unknown Exception";
                    ViewBag.ErrorType = "danger";

                    // return View wont work because i need user data again, but cant use redirect because i need to show error message
                    return RedirectToAction("Account", "User");
                    //return View("Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
    }
}