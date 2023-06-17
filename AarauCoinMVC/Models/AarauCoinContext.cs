using Microsoft.EntityFrameworkCore;

namespace AarauCoinMVC.Models
{
    public class AarauCoinContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Coin> Coins { get; set; }
        public DbSet<Level> Levels { get; set; }

        public AarauCoinContext()
        {
        }

        public AarauCoinContext(DbContextOptions<AarauCoinContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "AuthorDb");
        }
    }
}