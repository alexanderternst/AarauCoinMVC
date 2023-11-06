using AarauCoin.Database;

namespace AarauCoin.Models
{
    public class UserWithCoin
    {
        public string Username { get; set; } = string.Empty;
        public double Coins { get; set; }
    }
}