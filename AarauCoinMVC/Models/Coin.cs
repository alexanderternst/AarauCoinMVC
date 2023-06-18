namespace AarauCoinMVC.Models
{
    public class Coin
    {
        public int Id { get; set; }
        public User UserId { get; set; }

        public double Coins { get; set; }
    }
}