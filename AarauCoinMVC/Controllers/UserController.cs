using AarauCoinMVC.Models;
using AarauCoinMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NuGet.Configuration;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

        #region Login/Logout

        public IActionResult Login()
        {
            _logger.LogInformation($"Login page says hello");
            return View();
        }

        public async Task<IActionResult> LoginAction(LoginViewModel loginData)
        {
            try
            {
                if (loginData == null)
                    throw new UserException("Login data is invalid");
                if (string.IsNullOrEmpty(loginData.Username) || string.IsNullOrEmpty(loginData.Username))
                    throw new UserException("Login failed, password or username are empty");

                var user = await _context.GetUser(loginData.Username);

                if (user == null)
                    throw new Exception();

                if (loginData.Username.ToLower() == user.Username.ToLower() && loginData.Password == user.Password)
                {
                    CreateLoginCookie(loginData, user.Level);
                    _logger.LogInformation($"User {loginData.Username} logged in");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    throw new UserException("Login failed, incorrect password or username");
                }
            }
            catch (UserException uex)
            {
                _logger.LogInformation($"User with {loginData.Username} failed to log in");
                ViewBag.ErrorMessage = uex.Message;
                ViewBag.ErrorType = "info";
                return View("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unknown Exception" + ex.Message);
                ViewBag.ErrorMessage = "Unknown error occured";
                ViewBag.ErrorType = "danger";
                return View("Login");
            }
        }

        private async Task CreateLoginCookie(LoginViewModel loginData, string level)
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

            await HttpContext.SignInAsync("AarauCoin-AuthenticationScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
            _logger.LogInformation($"Authentification cookie for user {loginData.Username} was created");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AarauCoin-AuthenticationScheme");
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Account Page
        public async Task<IActionResult> Account()
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    AccountViewModel? userInformation = await ShowAccount();

                    _logger.LogInformation($"User {User.Identity.Name} loaded account page");
                    return View("Account", userInformation);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    //ViewBag.ErrorMessage = "Unknown error occured";
                    //ViewBag.ErrorType = "danger";
                    return RedirectToAction("Account", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public async Task<IActionResult> AdminAccount()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                try
                {
                    AccountViewModel? userInformation = await ShowAccount();
                    
                    List<AdminAccountViewModel> allUsers = await _context.GetAllUsers();
                    if (allUsers != null)
                    {
                        userInformation.AllAccounts = allUsers;
                    }

                    _logger.LogInformation($"User {User.Identity.Name} loaded admin account page");
                    return View("AdminAccount", userInformation);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    //ViewBag.ErrorMessage = "Unknown error occured";
                    //ViewBag.ErrorType = "danger";
                    return RedirectToAction("Account", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        private async Task<AccountViewModel?> ShowAccount()
        {
            if (TempData.ContainsKey("ErrorMessage"))
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                ViewBag.ErrorType = TempData["ErrorType"];
            }

            AccountViewModel? userInformation = await _context.GetUserInformation(User.Identity.Name);

            List<string> users = await _context.GetUserNames();
            if (users != null)
            {
                users.Remove(User.Identity.Name.ToString());
                userInformation.Users = users.ToArray();
            }
            return userInformation;
        }
        #endregion

        #region Admin Features
        public async Task<IActionResult> ModifyUser(string username, double coins)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                try
                {
                    if (coins <= 0)
                        throw new UserException("Amount must be greater than 0");

                    await _context.ModifyUser(username, coins);
                    return RedirectToAction("AdminAccount", "User");
                }
                catch (UserException uex)
                {
                    _logger.LogInformation(uex.Message);
                    SaveTempData(uex.Message, "info");
                    return RedirectToAction("AdminAccount", "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    SaveTempData(ex.Message, "danger");
                    return RedirectToAction("AdminAccount", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        

        public async Task<IActionResult> CreateUser(string username, string password, string level, double coins)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                try
                {
                    string regex = "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$";
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(level))
                        throw new UserException("Username, Password or level is null or empty");
                    if (coins <= 0)
                        throw new UserException("Coins must be greater than 0");
                    if (!Regex.IsMatch(password, regex))
                        throw new UserException("Password is invalid");


                    await _context.CreateUser(username, password, level, coins);
                    _logger.LogInformation($"User with username {username} created");

                    return RedirectToAction("AdminAccount", "User");
                }
                catch (UserException uex)
                {
                    _logger.LogError(uex.Message);
                    SaveTempData(uex.Message, "info");

                    return RedirectToAction("AdminAccount", "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    SaveTempData(ex.Message, "danger");

                    return RedirectToAction("AdminAccount", "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
        #endregion

        #region Send Money
        public async Task<IActionResult> SendMoney(string reciever, double amount)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    if (amount <= 1)
                        throw new UserException("Amount must be greater than 1");

                    await _context.SendMoney(User.Identity.Name, reciever, amount);
                    _logger.LogInformation($"User {User.Identity.Name} sent {amount} coins to {reciever}");

                    return RedirectToAction(ReturnPage(), "User");
                }
                catch (UserException uex)
                {
                    _logger.LogError(uex.Message);
                    SaveTempData(uex.Message, "info");
                    return RedirectToAction(ReturnPage(), "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    SaveTempData(ex.Message, "danger");
                    return RedirectToAction(ReturnPage(), "User");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
        #endregion

        private void SaveTempData(string errorMessage, string errorType)
        {
            TempData["ErrorMessage"] = errorMessage;
            TempData["ErrorType"] = errorType;
        }

        private string ReturnPage()
        {
            if (User.IsInRole("Admin"))
                return "AdminAccount";
            else
                return "Account";
        }
    }
}