namespace AarauCoinMVC.Models
{
    public class AccountViewModel
    {
        public string Username { get; set; }
        public string Level { get; set; }
        public Coin? Coins { get; set; }

        public string[] Users { get; set; }
    }
}