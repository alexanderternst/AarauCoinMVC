using AarauCoinMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            bool isLoggedIn = false; // determine users authentication status
            ViewBag.IsLoggedIn = isLoggedIn;
            return View("Index");
        }

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
            // replace with actual login code
            ViewBag.IsLoggedIn = true; // only do if login is successful
            return View("Index");
        }

        public IActionResult LogoutAction(string username)
        {
            // replace with actual logout code
            ViewBag.IsLoggedIn = false; // only do if logout is successful
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}