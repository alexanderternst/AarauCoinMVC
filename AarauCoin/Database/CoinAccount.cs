using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarauCoin.Database
{
    public class CoinAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public User User { get; set; } = new(); // setting this to nothing does not work cause of null warning, but null! does not make a lot of sense

        public double Coins { get; set; }
    }
}