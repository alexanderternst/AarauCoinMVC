using System.ComponentModel.DataAnnotations;

namespace AarauCoin.Models
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,16}$", ErrorMessage = "Password is invalid, it must meet our guidelines which you can see on the registration page")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Level is required.")]
        public string Level { get; set; } = string.Empty;

        [Required(ErrorMessage = "Coins are required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double Coins { get; set; }
    }
}
