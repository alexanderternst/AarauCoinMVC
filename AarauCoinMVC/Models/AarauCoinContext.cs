using Microsoft.EntityFrameworkCore;

namespace AarauCoinMVC.Models
{
    public class AarauCoinContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "AuthorDb");
        }   
    }
}
