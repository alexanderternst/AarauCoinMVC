using Microsoft.EntityFrameworkCore;

namespace AarauCoin.Database
{
    public class AarauCoinContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<CoinAccount> CoinAccounts { get; set; }
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
            optionsBuilder.UseInMemoryDatabase(databaseName: "AarauCoinDb");
            //InsertLevels();
        }

        //// Add a method to seed data into the Levels table
        //public void InsertLevels()
        //{
        //    if (!Levels.Any())
        //    {
        //        // Create and add Level entities
        //        var levels = new Level[]
        //        {
        //            new Level { Id = 1, Name = "Admin"},
        //            new Level { Id = 2, Name = "User"},
        //            // Add more levels as needed
        //        };

        //        Levels.AddRange(levels);
        //        SaveChanges();
        //    }
        //}
    }
}