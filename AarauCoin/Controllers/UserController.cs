using AarauCoin.Models;
using AarauCoin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AarauCoin.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<UserController> _logger;
        private readonly UserService _service;

        /// <summary>
        /// Konstruktor für den User controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public UserController(ILogger<UserController> logger, UserService service, IHttpContextAccessor httpContextAccessor)
        {
            _contextAccessor = httpContextAccessor;
            _service = service;
            _logger = logger;
        }

        #region Login/Logout

        /// <summary>
        /// Methode für den Aufruf der Login Seite
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            _logger.LogInformation($"Login page says hello");
            return View();
        }

        /// <summary>
        /// Methode für das ausführen der Login Aktion
        /// </summary>
        /// <param name="loginData"></param>
        /// <returns></returns>
        public async Task<IActionResult> Login(UserIndexViewModel user)
        {
            if (_contextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true)
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

                    var loginSuccess = _service.VerifyPassword(dbUser.Salt, dbUser.Password, user.Password);

                    var username = dbUser.Username;
                    if (loginSuccess)
                    {
                        await CreateLoginCookie(dbUser.Username, dbUser.Level.Name);
                        _logger.LogInformation("User {username} logged in", username);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
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
                ExpiresUtc = DateTime.UtcNow.AddMinutes(1)
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

        //#region Account Page

        ///// <summary>
        ///// Methode für das anzeigen der Account Seite, Übergeben von Daten zur Page zum anzeigen
        ///// </summary>
        ///// <returns></returns>
        //public async Task<IActionResult> Account()
        //{
        //    if (_contextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true)
        //    {
        //        try
        //        {
        //            AccountViewModel? userInformation = await ShowAccount();

        //            _logger.LogInformation($"User {User.Identity.Name} loaded account page");
        //            return View("Account", userInformation);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError("Unknown Exception " + ex.Message);
        //            //ViewBag.ErrorMessage = "Unknown error occured";
        //            //ViewBag.ErrorType = "danger";
        //            return RedirectToAction("Account", "User");
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "User");
        //    }
        //}

        ///// <summary>
        ///// Methode für das anzeigen der Admin Account Seite, Übergeben von Daten zur Page zum anzeigen
        ///// </summary>
        ///// <returns></returns>
        //public async Task<IActionResult> AdminAccount()
        //{
        //    if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        //    {
        //        try
        //        {
        //            AccountViewModel? userInformation = await ShowAccount();

        //            List<AdminAccountViewModel> allUsers = await _context.GetAllUsers();
        //            if (allUsers != null)
        //            {
        //                userInformation.AllAccounts = allUsers;
        //            }

        //            _logger.LogInformation($"User {User.Identity.Name} loaded admin account page");
        //            return View("AdminAccount", userInformation);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError("Unknown Exception " + ex.Message);
        //            //ViewBag.ErrorMessage = "Unknown error occured";
        //            //ViewBag.ErrorType = "danger";
        //            return RedirectToAction("Account", "User");
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "User");
        //    }
        //}

        ///// <summary>
        ///// Auslesen der Daten für die Account Seite (Admin und User). Aufruf des Services für die durchsetzung der Datenabfrage
        ///// </summary>
        ///// <returns></returns>
        //private async Task<AccountViewModel?> ShowAccount()
        //{
        //    if (TempData.ContainsKey("ErrorMessage"))
        //    {
        //        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        //        ViewBag.ErrorType = TempData["ErrorType"];
        //    }

        //    AccountViewModel? userInformation = await _context.GetUserInformation(User.Identity.Name);

        //    List<string> users = await _context.GetUserNames();
        //    if (users != null)
        //    {
        //        users.Remove(User.Identity.Name.ToString());
        //        userInformation.Users = users.ToArray();
        //    }
        //    return userInformation;
        //}

        //#endregion Account Page

        //#region Admin Features

        ///// <summary>
        ///// Methode für das hinzufügen von Coins zu einem User durch Service.
        ///// Nur für Admin Benutzer möglich
        ///// </summary>
        ///// <param name="username"></param>
        ///// <param name="coins"></param>
        ///// <returns></returns>
        //public async Task<IActionResult> ModifyUser(string username, double coins)
        //{
        //    if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        //    {
        //        try
        //        {
        //            if (coins <= 0)
        //                throw new UserException("Amount must be greater than 0");

        //            await _context.ModifyUser(username, coins);
        //            return RedirectToAction("AdminAccount", "User");
        //        }
        //        catch (UserException uex)
        //        {
        //            _logger.LogInformation(uex.Message);
        //            SaveTempData(uex.Message, "info");
        //            return RedirectToAction("AdminAccount", "User");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError("Unknown Exception " + ex.Message);
        //            SaveTempData("Unknown error occured", "danger");
        //            return RedirectToAction("AdminAccount", "User");
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "User");
        //    }
        //}

        ///// <summary>
        ///// Methode für das erstellen eines neuen Users durch Service.
        ///// Nur für Admin Benutzer möglich
        ///// </summary>
        ///// <param name="username"></param>
        ///// <param name="password"></param>
        ///// <param name="level"></param>
        ///// <param name="coins"></param>
        ///// <returns></returns>
        //public async Task<IActionResult> CreateUser(string username, string password, string level, double coins)
        //{
        //    if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        //    {
        //        try
        //        {
        //            string regex = "^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$";
        //            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(level))
        //                throw new UserException("Username, Password or level is null or empty");
        //            if (coins <= 0)
        //                throw new UserException("Coins must be greater than 0");
        //            if (!Regex.IsMatch(password, regex))
        //                throw new UserException("Password is invalid");

        //            await _context.CreateUser(username, password, level, coins);
        //            _logger.LogInformation($"User with username {username} created");

        //            return RedirectToAction("AdminAccount", "User");
        //        }
        //        catch (UserException uex)
        //        {
        //            _logger.LogError(uex.Message);
        //            SaveTempData(uex.Message, "info");

        //            return RedirectToAction("AdminAccount", "User");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError("Unknown Exception  " + ex.Message);
        //            SaveTempData("Unknown error occured", "danger");

        //            return RedirectToAction("AdminAccount", "User");
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "User");
        //    }
        //}

        //#endregion Admin Features

        //#region Send Money

        ///// <summary>
        ///// Methode für das senden von Coins an einen anderen User durch Service, möglich für alle Benutzer
        ///// </summary>
        ///// <param name="reciever"></param>
        ///// <param name="amount"></param>
        ///// <returns></returns>
        //public async Task<IActionResult> SendMoney(string reciever, double amount)
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        try
        //        {
        //            if (amount <= 1)
        //                throw new UserException("Amount must be greater than 1");

        //            await _context.SendMoney(User.Identity.Name, reciever, amount);
        //            _logger.LogInformation($"User {User.Identity.Name} sent {amount} coins to {reciever}");

        //            return RedirectToAction(ReturnPage(), "User");
        //        }
        //        catch (UserException uex)
        //        {
        //            _logger.LogError(uex.Message);
        //            SaveTempData(uex.Message, "info");
        //            return RedirectToAction(ReturnPage(), "User");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError("Unknown Exception " + ex.Message);
        //            SaveTempData("Unknown error occured", "danger");
        //            return RedirectToAction(ReturnPage(), "User");
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "User");
        //    }
        //}

        //#endregion Send Money

        ///// <summary>
        ///// Methode für das speichern von TempData zur temporären Anzeige von Fehlermeldungen
        ///// </summary>
        ///// <param name="errorMessage"></param>
        ///// <param name="errorType"></param>
        //private void SaveTempData(string errorMessage, string errorType)
        //{
        //    TempData["ErrorMessage"] = errorMessage;
        //    TempData["ErrorType"] = errorType;
        //}

        ///// <summary>
        ///// Methode für das zurückgeben der richtigen Seite, je nach Rolle des Benutzers
        ///// </summary>
        ///// <returns></returns>
        //private string ReturnPage()
        //{
        //    if (User.IsInRole("Admin"))
        //        return "AdminAccount";
        //    else
        //        return "Account";
        //}
    }
}