using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarauCoin.Database
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        
        public string Password { get; set; } = string.Empty;

        public string Salt { get; set; } = string.Empty;

        public Level Level { get; set; } = new();
    }
}