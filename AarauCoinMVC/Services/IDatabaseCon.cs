using AarauCoinMVC.Models;
using AarauCoinMVC.Models.Database;

namespace AarauCoinMVC.Services
{
    public interface IDatabaseCon
    {
        Task<User?> Login(string username);
        
        bool VerifyPassword(string storedSalt, string storedHashedPassword, string attemptedPassword);

        Task<AccountViewModel?> GetUserInformation(string username);

        Task<List<LogViewModel>> ReadLog(string date);

        Task<List<string>> GetUserNames();

        Task<List<AdminAccountViewModel>> GetAllUsers();

        Task ModifyUser(string username, double coins);

        Task SendMoney(string sender, string receiver, double amount);

        Task CreateUser(string username, string password, string level, double coins);

        List<LogViewModel> ParseLog(string fileContent);
    }
}