using AarauCoin.Database;

namespace AarauCoin.Models
{
    public class AccountViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public CoinAccount? Coins { get; set; }

        public IEnumerable<string>? Users { get; set; }
        public IEnumerable<UserWithCoin>? AllAccounts { get; set; }
    }

    public class UserWithCoin
    {
        public string Username { get; set; } = string.Empty;
        public double Coins { get; set; }
    }
}