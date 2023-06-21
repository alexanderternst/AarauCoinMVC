using AarauCoinMVC.Models;
using AarauCoinMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace AarauCoinMVC.Controllers
{
    public class LogEntryController : Controller
    {
        private readonly ILogger<LogEntryController> _logger;
        private readonly IDatabaseCon _context;

        /// <summary>
        /// Konstruktor für den LogEntry controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public LogEntryController(ILogger<LogEntryController> logger, IDatabaseCon context)
        {
            ViewBag.ErrorMessage = null;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Methode für den Aufruf der Log Seite
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Methode für die Anzeige der Log Einträge
        /// </summary>
        /// <param name="date"></param>
        /// <param name="searchContent"></param>
        /// <param name="picker"></param>
        /// <returns></returns>
        public async Task<IActionResult> ShowLog(DateTime date, string searchContent, string picker)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                try
                {
                    string datestring = date.ToString("yyyyMMdd");
                    List<LogViewModel> logs = await _context.ReadLog(datestring);

                    var filteredLogs = logs
                        .Where(
                            s => string.IsNullOrWhiteSpace(searchContent) ||
                            s.LogMessage.ToLower().Contains(searchContent.ToLower().Trim())
                                );

                    if (picker == "Newest")
                        logs = filteredLogs.OrderByDescending(s => s.LogDate).ToList();
                    if (picker == "Oldest")
                        logs = filteredLogs.OrderBy(s => s.LogDate).ToList();

                    _logger.LogInformation("Logs successfully loaded");
                    return View("Log", logs);
                }
                catch (FileNotFoundException ex)
                {
                    _logger.LogError("File not found " + ex.Message);
                    ViewBag.ErrorMessage = "File not found";
                    ViewBag.ErrorType = "danger";
                    return View("Log");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown Exception " + ex.Message);
                    ViewBag.ErrorMessage = "Unknown error occured";
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