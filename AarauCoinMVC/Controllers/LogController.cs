using AarauCoinMVC.Models;
using AarauCoinMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace AarauCoinMVC.Controllers
{
    public class LogController : Controller
    {
        private readonly ILogger<LogController> _logger;
        private readonly IDatabaseCon _context;

        public LogController(ILogger<LogController> logger, IDatabaseCon context)
        {
            ViewBag.ErrorMessage = null;
            _context = context;
            _logger = logger; 
        }

        public IActionResult Log()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                _logger.LogInformation("Log page says hello");
                return View();
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        public IActionResult ShowLog(DateTime date, string searchContent)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
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

                    _logger.LogInformation("Logs successfully loaded");
                    return View("Log", logs);
                }
                catch (FileNotFoundException ex)
                {
                    _logger.LogError("File not found" + ex.Message);
                    ViewBag.ErrorMessage = "File not found";
                    ViewBag.ErrorType = "danger";
                    return View("Log");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception" + ex.Message);
                    ViewBag.ErrorMessage = "Unknown Exception";
                    ViewBag.ErrorType = "danger";
                    return View("Log");
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }
    }
}