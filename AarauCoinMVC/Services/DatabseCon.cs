using AarauCoinMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace AarauCoinMVC.Services
{
    public class DatabseCon : IDatabaseCon
    {

        private readonly AarauCoinContext _context;
        public DatabseCon(AarauCoinContext context)
        {

            _context = context;

            List<User> user = _context.Users.ToList();
            List<Level> lev = _context.Levels.ToList();

            if (user.Count == 0 && lev.Count == 0)
            {
                _context.Levels.Add(new Level
                {
                    LevelId = 1,
                    LevelName = "Admin"
                });
                _context.SaveChanges();

                _context.Levels.Add(new Level
                {
                    LevelId = 2,
                    LevelName = "User"
                });
                _context.SaveChanges();

                _context.Users.Add(new User
                {
                    Username = "Hans",
                    Password = "123",
                    LevelId = _context.Levels.FirstOrDefault(s => s.LevelName == "User")
                });
                _context.SaveChanges();

                _context.Users.Add(new User
                {
                    Username = "Alex",
                    Password = "123",
                    LevelId = _context.Levels.FirstOrDefault(s => s.LevelName == "Admin")
                });
                _context.SaveChanges();
            }
        }
        public UserLoginDTO GetUser(LoginModel loginData)
        {
            UserLoginDTO list = _context.Users.
                    Select(e => new UserLoginDTO
                    {
                        Id = e.UserId,
                        Username = e.Username,
                        Password = e.Password,
                        Level = e.LevelId.LevelName
                    }).First(s => s.Username == loginData.Username);
            return list;
        }
    }
}
