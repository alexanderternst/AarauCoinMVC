using AarauCoinMVC.Models;

namespace AarauCoinMVC.Services
{
    public class DatabseCon : IDatabaseCon
    {
        private readonly AarauCoinContext _context;
        private readonly ILogger<DatabseCon> _logger;

        public DatabseCon(AarauCoinContext context, ILogger<DatabseCon> logger)
        {
            _context = context;
            _logger = logger;
            InsertUser();
        }

        private void InsertUser()
        {
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

                _context.Coins.Add(new Coin
                {
                    Id = 1,
                    Coins = 1000,
                    UserId = _context.Users.FirstOrDefault(m => m.Username == "Hans")
                });
                _context.SaveChanges();
            }
        }

        public UserLoginDTO? GetUser(string username)
        {
            try
            {
                UserLoginDTO? list = _context.Users
                    .Select(e => new UserLoginDTO
                    {
                        Id = e.UserId,
                        Username = e.Username,
                        Password = e.Password,
                        Level = e.LevelId.LevelName,
                        Coins = _context.Coins.Where(s => s.UserId.Username == username).FirstOrDefault()
                    }).FirstOrDefault(s => s.Username.ToLower() == username.ToLower());
                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unknown Exception" + ex.Message);
                return null;
            }
        }

        public AccountViewModel? GetUserInformation(string username)
        {
            AccountViewModel? list = _context.Users.
                  Select(e => new AccountViewModel
                  {
                      Username = e.Username,
                      Level = e.LevelId.LevelName,
                      Coins = _context.Coins.Where(s => s.UserId.Username == username).FirstOrDefault(),
                  }).FirstOrDefault(s => s.Username.ToLower() == username.ToLower());
            return list;
        }

        public List<LogViewModel> ReadLog(string date)
        {
            string fileName = $"../logs/webapi-{date}.log";

            if (!File.Exists(fileName))
                throw new FileNotFoundException("Datei nicht gefunden", fileName);

            string fileContent = string.Empty;
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    StreamReader streamReader = new StreamReader(fileStream);
                    fileContent = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Unknown Exception" + ex.Message);
                return null;
            }

            List<LogViewModel> list = new List<LogViewModel>();

            foreach (var line in fileContent.Split("\n"))
            {
                var data = line.Split("]");

                if (data.Length < 2)
                    continue;

                string datum = data[0].Remove(0, 1).Remove(31, 4);
                DateTime parsedDatum;
                string message = data[1];
                string type = data[0].Remove(0, 33);


                if (DateTime.TryParse(datum, out parsedDatum))
                {
                    LogViewModel log = new LogViewModel() { LogDate = parsedDatum, TypeOfLog = type, LogMessage = message };
                    list.Add(log);
                }
                else
                {
                    continue;
                }
            }

            return list;
        }
    }
}