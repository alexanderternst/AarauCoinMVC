namespace AarauCoin.Models
{
    public class LogViewModel
    {
        public DateTimeOffset LogDate { get; set; }
        public string TypeOfLog { get; set; } = string.Empty;
        public string LogMessage { get; set; } = string.Empty;
    }
}