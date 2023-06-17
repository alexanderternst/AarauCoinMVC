using AarauCoinMVC.Models;

namespace AarauCoinMVC.Services
{
    public interface IDatabaseCon
    {
        public UserLoginDTO GetUser(LoginModel loginData);
        public List<LogViewModel> ReadLog(string date);
    }
}
