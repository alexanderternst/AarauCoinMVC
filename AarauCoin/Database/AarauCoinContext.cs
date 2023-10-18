using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace AarauCoin.Database
{
    public class AarauCoinContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<CoinAccount> CoinAccounts { get; set; }
        public DbSet<Level> Levels { get; set; }

        private readonly IConfiguration _configuration;

        public AarauCoinContext(DbContextOptions<AarauCoinContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //// In-Memory
            optionsBuilder.UseInMemoryDatabase(databaseName: "AarauCoinDb");

            //// MariaDb code
            //var connectionString = _configuration.GetConnectionString("AarauCoinDb") ?? string.Empty;
            //var serverVersion = ServerVersion.AutoDetect(connectionString);
            //optionsBuilder.UseMySql(connectionString, serverVersion);
        }
    }
}