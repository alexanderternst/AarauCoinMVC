using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarauCoin.Database
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Salt { get; set; } = string.Empty;

        [MaxLength(255)]
        public string HashingAlgorithm { get; set; } = string.Empty;

        public Level Level { get; set; } = new();
    }
}