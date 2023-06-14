using AarauCoinMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace AarauCoinMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        //[Authorize] -- Does not work yet probably needs more arguments
        public IActionResult Privacy()
        {
            // Knows to return Privacy View because of the name of the method
            // This works because HomeController inherits from Controller
            // We can also specify the name of the View to return like we did in Index()
            return View();
        }

        public IActionResult Login()
        {
            // open login page
            // we can either do if statement here or in cshtml (to check if user is logged in and return appropriate view)
            return View();
        }

        public IActionResult LoginAction(string username, string password)
        {
            // TODO: Program actual login code here
            // TODO: Only do this if login is successful
            // TODO: Save user rights and username in Claim
            // TODO: Add idle timeout
            // TODO: Only show Log in Navbar when Admin User is logged in (If Statement in cshtml of Shared Layout)

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Username"), // Add claims as needed
                new Claim (ClaimTypes.Role, "Role") // Add claims as needed
            };

            var claimsIdentity = new ClaimsIdentity(claims, "YourAuthenticationScheme");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Set whether the cookie should persist across sessions
                ExpiresUtc = DateTime.UtcNow.AddMinutes(1) // Set the expiration time of the cookie
            };

            HttpContext.SignInAsync("YourAuthenticationScheme", new ClaimsPrincipal(claimsIdentity), authProperties);

            // WARNING: DO NOT USE THIS CODE IN PRODUCTION
            // GET DATA FROM COOKIE
            var a = User.Identity.Name;
            var b = User.Identity.IsAuthenticated;
            var c = User.Identity.AuthenticationType;
            var d = User.Identity.Name;
            var e = User.FindFirst(ClaimTypes.Role);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult LogoutAction(string username)
        {
            // TODO: Only do this if logout is successful

            HttpContext.SignOutAsync("YourAuthenticationScheme");

            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}