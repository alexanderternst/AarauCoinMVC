using AarauCoinMVC.Models;
using AarauCoinMVC.Models.Database;
using Microsoft.EntityFrameworkCore;

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

        #region Insert data

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
                    Password = "ibz1234",
                    LevelId = _context.Levels.FirstOrDefault(s => s.LevelName == "User")
                });
                _context.SaveChanges();

                _context.Users.Add(new User
                {
                    Username = "Alex",
                    Password = "ibz1234",
                    LevelId = _context.Levels.FirstOrDefault(s => s.LevelName == "Admin")
                });
                _context.SaveChanges();

                _context.Coins.Add(new Coin
                {
                    Coins = 1000,
                    UserId = _context.Users.FirstOrDefault(m => m.Username == "Hans")
                });
                _context.SaveChanges();

                _context.Coins.Add(new Coin
                {
                    Coins = 1000,
                    UserId = _context.Users.FirstOrDefault(m => m.Username == "Alex")
                });
                _context.SaveChanges();
            }
        }

        #endregion Insert data

        #region Get user data

        public async Task<UserLoginDTO?> GetUser(string username)
        {
            UserLoginDTO? user = await _context.Users
                .Select(e => new UserLoginDTO
                {
                    Id = e.UserId,
                    Username = e.Username,
                    Password = e.Password,
                    Level = e.LevelId.LevelName,
                    Coins = _context.Coins.Where(s => s.UserId.Username == username).FirstOrDefault()
                }).FirstOrDefaultAsync(s => s.Username.ToLower() == username.ToLower());
            return user;
        }

        public async Task<List<string>> GetUserNames()
        {
            return await _context.Users.Select(s => s.Username).ToListAsync();
        }

        public async Task<AccountViewModel?> GetUserInformation(string username)
        {
            AccountViewModel? user = await _context.Users.
                  Select(e => new AccountViewModel
                  {
                      Username = e.Username,
                      Level = e.LevelId.LevelName,
                      Coins = _context.Coins.Where(s => s.UserId.Username == username).FirstOrDefault(),
                  }).FirstOrDefaultAsync(s => s.Username.ToLower() == username.ToLower());
            return user;
        }

        public async Task<List<AdminAccountViewModel>> GetAllUsers()
        {
            return await _context.Users.Select(e => new AdminAccountViewModel
            {
                Username = e.Username,
                Coins = _context.Coins.Where(s => s.UserId.Username == e.Username).First().Coins
            }).ToListAsync();
        }

        #endregion Get user data

        #region Set user data

        public async Task CreateUser(string username, string password, string level, double coins)
        {
            await _context.Users.AddAsync(
                new User
                {
                    Username = username,
                    Password = password,
                    LevelId = _context.Levels.First(s => s.LevelName == level)
                });
            await _context.SaveChangesAsync();

            await _context.Coins.AddAsync(
                 new Coin
                 {
                     Coins = coins,
                     UserId = _context.Users.First(s => s.Username == username)
                 });
            await _context.SaveChangesAsync();
        }

        public async Task ModifyUser(string username, double coins)
        {
            _context.Coins.Where(s => s.UserId.Username == username).First().Coins = coins;
            await _context.SaveChangesAsync();
        }

        #endregion Set user data

        #region Send money

        public async Task SendMoney(string sender, string receiver, double amount)
        {
            var senderAccount = await _context.Coins.FirstOrDefaultAsync(s => s.UserId.Username == sender);
            var receiverAccount = await _context.Coins.FirstOrDefaultAsync(s => s.UserId.Username == receiver);

            if (senderAccount == null)
                throw new UserException("Sender has no account");

            if (receiverAccount == null)
                throw new UserException("Receiver has no account");

            if (senderAccount.Coins < amount)
                throw new UserException("Not enough coins");

            senderAccount.Coins -= amount;
            receiverAccount.Coins += amount;

            await _context.SaveChangesAsync();
        }

        #endregion Send money

        #region Log

        public async Task<List<LogViewModel>> ReadLog(string date)
        {
            string fileName = $"../logs/webapi-{date}.log";

            if (!File.Exists(fileName))
                throw new FileNotFoundException("Datei nicht gefunden", fileName);

            string fileContent = string.Empty;
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StreamReader streamReader = new StreamReader(fileStream);
                fileContent = await streamReader.ReadToEndAsync();
            }
            return ParseLog(fileContent);
        }

        public List<LogViewModel> ParseLog(string fileContent)
        {
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

        #endregion Log
    }
}