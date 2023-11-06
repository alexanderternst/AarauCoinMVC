using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarauCoinMVC.Models.Database
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        // NEW
        public string Salt { get; set; }

        public Level? LevelId { get; set; }


    }
}