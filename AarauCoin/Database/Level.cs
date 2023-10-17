using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AarauCoin.Database
{
    public class Level
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}