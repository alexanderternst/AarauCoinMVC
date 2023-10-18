using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AarauCoin.Database
{
    public class FailedLoginAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; } // also could use foreign key

        public DateTimeOffset Time { get; set; }

        public TimeSpan Offset { get; set; }
    }
}
