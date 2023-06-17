using AarauCoinMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using AarauCoinMVC.Services;
using System.IO;

namespace AarauCoinMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDatabaseCon _context;

        public HomeController(ILogger<HomeController> logger, IDatabaseCon context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Log()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult ShowLog(DateTime date, string searchContent)
        {
            try
            {
                string datestring = date.ToString("yyyyMMdd");
                List<LogViewModel> logs = _context.ReadLog(datestring);
                var filteredLogs = logs
                    .Where(
                        s => string.IsNullOrWhiteSpace(searchContent) ||
                        s.LogMessage.ToLower().Contains(searchContent.ToLower().Trim())
                            );
                logs = filteredLogs.ToList();

                //ViewBag.Logs = logs;
                return View("Log", logs);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError("File not found" + ex.Message);
                // Handle error
                return RedirectToAction("Log");
            }
            catch (IOException ex)
            {
                _logger.LogError("IO Exception" + ex.Message);
                // Handle error
                return RedirectToAction("Log");

            }
            catch (Exception ex)
            {
                _logger.LogError("Unknown Exception" + ex.Message);
                // Handle error
                return RedirectToAction("Log");
            }
        }

        //public IActionResult SearchLog(string searchContent)
        //{
        //    List<LogViewModel> logs = ViewBag.Logs;
        //    logs = (List<LogViewModel>)logs.Where(s => s.LogMessage.Contains(searchContent)); ;
        //    return View("Log", logs);
        //}


        public IActionResult Index()
        {
            _logger.LogInformation("Index page says hello");
            return View("Index");
        }

        //[Authorize] -- Does not work yet probably needs more arguments
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page says hello");
            // Knows to return Privacy View because of the name of the method
            // This works because HomeController inherits from Controller
            // We can also specify the name of the View to return like we did in Index()
            return View();
        }

        public IActionResult Login()
        {
            _logger.LogInformation($"Login page says hello");
            // open login page
            // we can either do if statement here or in cshtml (to check if user is logged in and return appropriate view)
            return View();
        }

        public IActionResult LoginAction(LoginModel loginData)
        {
            var list = _context.GetUser(loginData);

            if (loginData.Username == list.Username && loginData.Password == list.Password)
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginData.Username), // Add claims as needed
                    new Claim (ClaimTypes.Role, list.Level) // Add claims as needed
                };

                var claimsIdentity = new ClaimsIdentity(claims, "AarauCoin-AuthenticationScheme");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Set whether the cookie should persist across sessions
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(1) // Set the expiration time of the cookie
                };

                HttpContext.SignInAsync("AarauCoin-AuthenticationScheme", new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        public IActionResult LogoutAction(string username)
        {
            //var a = HttpContext.User.Claims.ToList();
            //var c = HttpContext.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Role);
            //var b = User.FindFirst(ClaimTypes.Role).Value.ToString();

            HttpContext.SignOutAsync("AarauCoin-AuthenticationScheme");
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}