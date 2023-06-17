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

        public List<LogViewModel> ReadLog(string date)
        {
            string fileName = $"../logs/webapi-{date}.log";
            
            if(!File.Exists(fileName))
                throw new FileNotFoundException("File not found", fileName);
            
            string fileContent = string.Empty;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StreamReader streamReader = new StreamReader(fileStream);
                fileContent = streamReader.ReadToEnd();
            }
            List<LogViewModel> list = new List<LogViewModel>();

            foreach (var line in fileContent.Split("\n"))
            {
                var data = line.Split("]");
                
                if (data.Length < 2)
                    continue;

                string datum = data[0];
                string message = data[1];
                LogViewModel log = new LogViewModel() { LogDate = datum, LogMessage = message };

                list.Add(log);
            }
            return list;
        }
    }
}
