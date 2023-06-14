namespace AarauCoinMVC.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Level LevelId { get; set; }
    }
}
