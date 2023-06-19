using AarauCoinMVC.Models;
using NuGet.Protocol.Plugins;

namespace AarauCoinMVC.Services
{
    public interface IDatabaseCon
    {
        public UserLoginDTO? GetUser(string username);

        public AccountViewModel? GetUserInformation(string username);

        public List<LogViewModel> ReadLog(string date);
        List<string> GetUserNames();

        void SendMoney(string sender, string receiver, double amount);

        void CreateUser(string username, string password, string level, double coins);
    }
}