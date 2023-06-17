using AarauCoinMVC.Models;

namespace AarauCoinMVC.Services
{
    public interface IDatabaseCon
    {
        public UserLoginDTO GetUser(LoginViewModel loginData);

        public List<LogViewModel> ReadLog(string date);
    }
}