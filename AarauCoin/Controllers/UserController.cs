using AarauCoin.Models;
using AarauCoin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AarauCoin.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _cA;
        private readonly ILogger<UserController> _logger;
        private readonly UserService _service;

        /// <summary>
        /// Konstruktor für den User controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public UserController(ILogger<UserController> logger, UserService service, IHttpContextAccessor httpContextAccessor)
        {
            _cA = httpContextAccessor;
            _service = service;
            _logger = logger;
        }

        #region Login/Logout

        /// <summary>
        /// Methode für den Aufruf der Login Seite
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            // this is a very, very temp. solution
            await _service.InsertUser();

            _logger.LogInformation($"Login page says hello");
            return View();
        }

        /// <summary>
        /// Methode für das ausführen der Login Aktion
        /// </summary>
        /// <param name="loginData"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserIndexViewModel user)
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index", "Home"); // maybe show error
            }

            try
            {
                // ModelState.AddModelError("Region", "Region is mandatory");
                if (ModelState.IsValid)
                {
                    user.Username = user.Username.Trim();
                    user.Password = user.Password.Trim();

                    var dbUser = await _service.Login(user.Username);

                    if (dbUser == null || string.IsNullOrEmpty(dbUser.Username))
                    {
                        user.Error = ("User does not exist", "warning");
                        return View("Index", user);
                    }

                    var username = dbUser.Username;

                    if (!_service.LoginSucess(dbUser.Id))
                    {
                        user.Error = ("To many login attempts, account login blocked for 30 minutes", "danger");
                        return View("Index", user);
                    }

                    var loginSuccess = _service.VerifyPassword(dbUser.Salt, dbUser.Password, user.Password);
                    
                    if (loginSuccess)
                    {
                        await CreateLoginCookie(dbUser.Username, dbUser.Level.Name);
                        _logger.LogInformation("User {username} logged in", username);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        await _service.AddFailedLoginAttempt(dbUser.Id, DateTimeOffset.UtcNow);

                        // add info that he failed to log in WITH UTC TIME
                        // ... maybe add how many login attempts he has left, although this might be to processor heavy
                        _logger.LogInformation("User {username} failed to log in", username);
                        user.Error = ("Login failed, incorrect password or username", "info");
                        return View("Index", user);
                    }
                }

                return View("Index", user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occured. {Message}", ex.Message);
                return BadRequest("An error occured"); // maybe show error on login page // Although this isnt bad cause only time we get errors is an sql error and then the website is down anyway
            }
        }

        /// <summary>
        /// Methode für das erstellen des Login Cookies
        /// </summary>
        /// <param name="loginData"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private async Task CreateLoginCookie(string username, string level)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim (ClaimTypes.Role, level)
                };

            var claimsIdentity = new ClaimsIdentity(claims, "AarauCoin-AuthenticationScheme");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
            };

            await HttpContext.SignInAsync("AarauCoin-AuthenticationScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
            _logger.LogInformation("Authentification cookie for user {username} was created", username);
        }

        /// <summary>
        /// Methode für das ausloggen und löschen des Cookies
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AarauCoin-AuthenticationScheme");
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Account Page

        /// <summary>
        /// Methode für das anzeigen der Account Seite, Übergeben von Daten zur Page zum anzeigen
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Account()
        {
            if (IsAuthenticated())
            {
                try
                {
                    var userInformation = await ShowAccount();
                    _logger.LogInformation("User {Name} loaded account page", _cA.HttpContext?.User.Identity?.Name);
                    return View("Account", userInformation);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception {Message}", ex.Message);
                    var userInfo = new UserAccountViewModel();
                    userInfo.Error = ("An error occured", "danger");
                    return View("Account", userInfo);
                }
            }
            else
            {
                return RedirectToAction("Index", "User");
            }
        }

        /// <summary>
        /// Methode für das anzeigen der Admin Account Seite, Übergeben von Daten zur Page zum anzeigen
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AdminAccount()
        {
            if (IsAuthenticated() && User.IsInRole("Admin"))
            {
                try
                {
                    var userInformation = await ShowAccount();
                    var allAccounts = await _service.GetAllUsers();

                    if (allAccounts != null)
                    {
                        userInformation.AllAccounts = allAccounts;
                    }

                    _logger.LogInformation("User {Name} loaded admin account page", _cA.HttpContext?.User.Identity?.Name);
                    return View("AdminAccount", userInformation);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occured. {Message}", ex.Message);
                    var userInfo = new UserAccountViewModel();
                    userInfo.Error = ("An error occured", "danger");
                    return View("AdminAccount", userInfo);
                }
            }
            else
            {
                return RedirectToAction("Index", "User");
            }
        }

        /// <summary>
        /// Auslesen der Daten für die Account Seite (Admin und User). Aufruf des Services für die durchsetzung der Datenabfrage
        /// </summary>
        /// <returns></returns>
        private async Task<UserAccountViewModel> ShowAccount()
        {
            var username = _cA.HttpContext?.User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                throw new Exception("Username is non existant");

            var userInformation = await _service.GetUserInformation(username);

            var users = _service.GetUserNames().ToList();
            if (users != null) // list of all usernames except current one SECURITY: DONT SHOW USERNAMES MAYBE REMOVE
            {
                users.Remove(username);
                userInformation.Users = users;
            }
            return userInformation;
        }

        #endregion Account Page

        #region Admin Features

        /// <summary>
        /// Methode für das hinzufügen von Coins zu einem User durch Service.
        /// Nur für Admin Benutzer möglich
        /// </summary>
        /// <param name="username"></param>
        /// <param name="coins"></param>
        /// <returns></returns>
        public async Task<IActionResult> ModifyUser(string username, double coins)
        {
            if (IsAuthenticated() && User.IsInRole("Admin"))
            {
                try
                {
                    if (coins <= 0)
                        throw new UserException("Amount must be greater than 0");

                    await _service.ModifyUser(username, coins);
                    return RedirectToAction("AdminAccount", "User");
                }
                catch (UserException uex)
                {
                    _logger.LogInformation("{Message}", uex.Message);
                    return BadRequest(uex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occured. {Message}", ex.Message);
                    return BadRequest("An error occured");
                }
            }
            else
            {
                return RedirectToAction("Index", "User");
            }
        }

        /// <summary>
        /// Methode für das erstellen eines neuen Users durch Service.
        /// Nur für Admin Benutzer möglich
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="level"></param>
        /// <param name="coins"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserAccountViewModel viewmodel)
        {
            if (IsAuthenticated() && User.IsInRole("Admin"))
            {
                var createUserViewModel = viewmodel.CreateUser;
                if (ModelState.IsValid)
                {
                    try
                    {
                        await _service.CreateUser(createUserViewModel.Username, createUserViewModel.Password, createUserViewModel.Level, createUserViewModel.Coins);
                        _logger.LogInformation("User with username {Username} created", createUserViewModel.Username);
                        return RedirectToAction("AdminAccount", "User");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Exception occured. {Message}", ex.Message);
                        return BadRequest("An error occured");
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return RedirectToAction("Index", "User");
            }
        }

        #endregion Admin Features

        #region Send Money

        /// <summary>
        /// Methode für das senden von Coins an einen anderen User durch Service, möglich für alle Benutzer
        /// </summary>
        /// <param name="reciever"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<IActionResult> SendMoney(string reciever, double amount)
        {
            if (IsAuthenticated())
            {
                try
                {
                    if (amount <= 1)
                        return BadRequest("Amount must be greater than 1");

                    var username = _cA.HttpContext?.User.Identity?.Name;
                    if (username == null)
                        return BadRequest("User does not exist");

                    await _service.SendMoney(username, reciever, amount);
                    _logger.LogInformation("User {username} sent {amount} coins to {reciever}", username, amount, reciever);

                    return RedirectToAction(ReturnPage(), "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occured. {Message}", ex.Message);
                    return BadRequest("An error occured");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        #endregion Send Money

        private bool IsAuthenticated()
        {
            if (_cA.HttpContext?.User.Identity?.IsAuthenticated == true) return true;
            return false;
        }

        /// <summary>
        /// Methode für das zurückgeben der richtigen Seite, je nach Rolle des Benutzers
        /// </summary>
        /// <returns></returns>
        private string ReturnPage()
        {
            if (User.IsInRole("Admin"))
                return "AdminAccount";
            else
                return "Account";
        }
    }
}