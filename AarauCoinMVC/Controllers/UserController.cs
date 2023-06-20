using AarauCoinMVC.Models;
using AarauCoinMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
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
                    throw new Exception("Login data is invalid");
                if (string.IsNullOrEmpty(loginData.Username) || string.IsNullOrEmpty(loginData.Username))
                    throw new Exception("Username or password are empty");

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
                    if (TempData.ContainsKey("ErrorMessage"))
                    {
                        ViewBag.ErrorMessage = TempData["ErrorMessage"];
                        ViewBag.ErrorType = TempData["ErrorType"];
                    }

                    AccountViewModel? userInformation = _context.GetUserInformation(User.Identity.Name);

                    List<string> users = _context.GetUserNames();
                    if (users != null)
                    {
                        users.Remove(User.Identity.Name.ToString());
                        userInformation.Users = users.ToArray();
                    }

                    if (User.IsInRole("Admin"))
                    {
                        _logger.LogInformation($"User {User.Identity.Name} loaded admin account page");
                        return View("AdminAccount", userInformation);
                    }
                    else
                    {
                        _logger.LogInformation($"User {User.Identity.Name} loaded account page");
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
                    if (amount <= 0)
                        throw new Exception("Amount must be greater than 0");

                    _context.SendMoney(User.Identity.Name, reciever, amount);
                    _logger.LogInformation($"User {User.Identity.Name} sent {amount} coins to {reciever}");
                    return RedirectToAction("Account", "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    TempData["ErrorMessage"] = "Unknown exception";
                    TempData["ErrorType"] = "danger";

                    return RedirectToAction("Account", "User");
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
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(level))
                        throw new Exception("Username, Password or level is null or empty");
                    if (coins <= 0)
                        throw new Exception("Coins must be greater than 0");

                    _context.CreateUser(username, password, level, coins);
                    _logger.LogInformation($"User with username {username} created");

                    return RedirectToAction("Account", "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    TempData["ErrorMessage"] = "Unknown exception";
                    TempData["ErrorType"] = "danger";

                    return RedirectToAction("Account", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
    }
}