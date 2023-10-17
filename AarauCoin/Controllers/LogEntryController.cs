using Microsoft.AspNetCore.Mvc;

namespace AarauCoin.Controllers
{
    public class LogEntryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
