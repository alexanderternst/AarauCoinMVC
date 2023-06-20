using AarauCoinMVC.Models;
using AarauCoinMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AarauCoinMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDatabaseCon _context;

        /// <summary>
        /// Konstruktor für den Home controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public HomeController(ILogger<HomeController> logger, IDatabaseCon context)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Methode für den Aufruf der Index Seite
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            _logger.LogInformation("Index page says hello");
            return View();
        }

        /// <summary>
        /// Methode für den Aufruf der Privacy Seite
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            _logger.LogInformation("Privacy page says hello");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}