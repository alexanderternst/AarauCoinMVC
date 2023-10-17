namespace AarauCoin.Models
{
    public class LogEntryIndexViewModel
    {
        public IEnumerable<LogViewModel>? Logs { get; set; }
        public (string message, string type) Error { get; set; } = ("", "");
    }
}
