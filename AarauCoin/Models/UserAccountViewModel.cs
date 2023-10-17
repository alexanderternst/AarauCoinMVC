using AarauCoin.Database;

namespace AarauCoin.Models
{
    public class UserAccountViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public CoinAccount? Coins { get; set; }

        public IEnumerable<string>? Users { get; set; }
        public IEnumerable<UserWithCoin>? AllAccounts { get; set; }
        public CreateUserViewModel CreateUser { get; set; } = new();

        public (string message, string type) Error { get; set; } = ("", "");
    }
}
