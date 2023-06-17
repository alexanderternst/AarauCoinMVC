using AarauCoinMVC.Models;

namespace AarauCoinMVC.Services
{
    public interface IDatabaseCon
    {
        public UserLoginDTO? GetUser(string username);

        public List<LogViewModel> ReadLog(string date);
    }
}