using AarauCoin.Models;
using AarauCoin.Services;
using Microsoft.AspNetCore.Mvc;

namespace AarauCoin.Controllers
{
    public class LogEntryController : Controller
    {
        private readonly ILogger<LogEntryController> _logger;
        private readonly IHttpContextAccessor _cA;
        private readonly LogEntryService _service;

        /// <summary>
        /// Konstruktor für den LogEntry controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public LogEntryController(ILogger<LogEntryController> logger, IHttpContextAccessor httpContextAccessor, LogEntryService service)
        {
            _logger = logger;
            _cA = httpContextAccessor;
            _service = service;
        }

        /// <summary>
        /// Methode für den Aufruf der Log Seite
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (IsAuthenticated() && User.IsInRole("Admin"))
            {
                _logger.LogInformation("Log page says hello");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "User");
            }
        }

        /// <summary>
        /// Methode für die Anzeige der Log Einträge
        /// TODO: Maybe duplicate file from current day and use that for current day
        /// </summary>
        /// <param name="date"></param>
        /// <param name="searchContent"></param>
        /// <param name="picker"></param>
        /// <returns></returns>
        public async Task<IActionResult> ShowLog(DateTime date, string searchContent, string picker)
        {
            if (IsAuthenticated() && User.IsInRole("Admin"))
            {
                try
                {
                    string datestring = date.ToString("yyyyMMdd");
                    
                    var logText = await _service.ReadLog(datestring);
                    var logs = _service.ParseLog(logText);

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

                    var viewModel = new LogEntryIndexViewModel();
                    viewModel.Logs = logs;
                    return View("Index", viewModel); // maybe try here to just return data and not a view, so it can be used as an api
                }
                catch (FileNotFoundException fex)
                {
                    _logger.LogError("File not found. {Message}", fex.Message);
                    var viewModel = new LogEntryIndexViewModel();
                    viewModel.Error = ("File not found, select another date (current date cannot be selected)", "info");
                    return View("Index", viewModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception occured. {Message}", ex.Message);
                    var viewModel = new LogEntryIndexViewModel();
                    viewModel.Error = ("An error occured", "danger");
                    return View("Index", viewModel);
                }
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }

        private bool IsAuthenticated()
        {
            if (_cA.HttpContext?.User.Identity?.IsAuthenticated == true)
                return true;
            return false;
        }
    }
}
