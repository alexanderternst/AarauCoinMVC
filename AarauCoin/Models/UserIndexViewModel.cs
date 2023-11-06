using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AarauCoin.Models
{
    public class UserIndexViewModel
    {
        [DisplayName("Name")]
        [Required(ErrorMessage = "Username is required.")]
        //[RegularExpression(@"\S", ErrorMessage = "Username cannot be empty or whitespace.")]
        public string Username { get; set; } = string.Empty;

        [DisplayName("Password")]
        [Required(ErrorMessage = "Password is required.")]
        //[RegularExpression(@"\S", ErrorMessage = "Password cannot be empty or whitespace.")]
        public string Password { get; set; } = string.Empty;

        public (string message, string type) Error { get; set; } = ("", "");
    }
}
