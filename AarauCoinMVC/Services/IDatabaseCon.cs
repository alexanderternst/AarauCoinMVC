using AarauCoinMVC.Models;

namespace AarauCoinMVC.Services
{
    public interface IDatabaseCon
    {
        UserLoginDTO? GetUser(string username);

        AccountViewModel? GetUserInformation(string username);

        List<LogViewModel> ReadLog(string date);

        List<string> GetUserNames();

        List<AdminAccountViewModel> GetAllUsers();

        void ModifyUser(string username, double coins);

        void SendMoney(string sender, string receiver, double amount);

        void CreateUser(string username, string password, string level, double coins);
    }
}