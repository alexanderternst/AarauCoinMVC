using AarauCoinMVC.Models;
using AarauCoinMVC.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AarauCoinMVC.Services
{
    public class DatabseCon : IDatabaseCon
    {
        private readonly AarauCoinContext _context;
        private readonly ILogger<DatabseCon> _logger;

        /// <summary>
        /// Konstruktor für die Datenbankverbindung und das Logging
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public DatabseCon(AarauCoinContext context, ILogger<DatabseCon> logger)
        {
            _context = context;
            _logger = logger;
            InsertUser();
        }

        #region Insert data

        /// <summary>
        /// Methode für das Einfügen von Daten in die In-Memory Datenbank
        /// </summary>
        private async void InsertUser()
        {
            IEnumerable<User> user = _context.Users;
            IEnumerable<Level> lev = _context.Levels;
            if (!user.Any() && !lev.Any())
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

                await CreateUser("Hans", "ibz1234", "User", 1000);

                await CreateUser("Alex", "ibz1234", "Admin", 1000);

                _context.Coins.Add(new Coin
                {
                    Coins = 1000,
                    UserId = _context.Users.First(m => m.Username == "Hans")
                });
                _context.SaveChanges();

                _context.Coins.Add(new Coin
                {
                    Coins = 1000,
                    UserId = _context.Users.First(m => m.Username == "Alex")
                });
                _context.SaveChanges();
            }
        }

        #endregion Insert data

        #region Get user data

        /// <summary>
        /// Methode für das Abrufen der Benutzerdaten mit praktisch allen Informationen
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<User?> Login(string username)
        {
            return await _context.Users.Include(x => x.LevelId).FirstOrDefaultAsync(s => s.Username.ToLower() == username.ToLower());
        }

        public bool VerifyPassword(string storedSalt, string storedHashedPassword, string attemptedPassword)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            var hash = HashPasswordWithSalt(salt, attemptedPassword);
            return hash.HashedPassword == storedHashedPassword; // store type of hashing in db
        }

        /// <summary>
        /// Methode für das Abrufen aller Benutzernamen
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetUserNames()
        {
            return await _context.Users.Select(s => s.Username).ToListAsync();
        }

        /// <summary>
        /// Methode für das Abrufen der Benutzerdaten mit den wichtigsten Informationen
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Methode für das Abrufen aller Benutzernamen und deren Coins
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Methode für das Erstellen eines neuen Benutzers mit Coins account
        /// Überprüfung ob der Benutzer bereits existiert
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="level"></param>
        /// <param name="coins"></param>
        /// <returns></returns>
        /// <exception cref="UserException">Wird geworfen wenn Benutzer bereits existiert</exception>
        public async Task CreateUser(string username, string password, string level, double coins)
        {
            var user = await _context.Users.FirstOrDefaultAsync(s => s.Username.ToLower() == username.ToLower());
            if (user != null)
                throw new UserException($"User with username {username} already exists");

            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var saltAndPassword = HashPasswordWithSalt(salt, password);

            await _context.Users.AddAsync(
                new User
                {
                    Username = username,
                    Password = saltAndPassword.HashedPassword,
                    Salt = saltAndPassword.Salt,
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

        /// <summary>
        /// Methode für das hinzufügen von Coins zu einem Benutzer durch den Admin.
        /// Zusätzliche Überprüfung ob der Benutzer existiert
        /// </summary>
        /// <param name="username"></param>
        /// <param name="coins"></param>
        /// <returns></returns>
        /// <exception cref="UserException">Wird geworfen wenn Benutzer nicht existiert</exception>
        public async Task ModifyUser(string username, double coins)
        {
            var reciever = await _context.Coins.FirstOrDefaultAsync(s => s.UserId.Username == username);
            if (reciever == null)
                throw new UserException("User has no account");

            _context.Coins.Where(s => s.UserId.Username == username).First().Coins = coins;
            await _context.SaveChangesAsync();
        }

        #endregion Set user data

        #region Send money

        /// <summary>
        /// Methode zum senden von Coins von einem Benutzer an einen anderen Benutzer
        /// Überprüfung ob Coin Accounts vorhanden sind und ob genug Coins vorhanden sind
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        /// <exception cref="UserException">Wird geworfen wenn Sender nicht existiert, Reciever nicht existiert oder wenn nicht genügend Coins vorhanden sind</exception>
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

        /// <summary>
        /// Methode welche die Log Datei durch eine Methode parsed und in eine Liste von LogViewModels umwandelt, und zuletzt zurückgibt
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Wird geworfen wenn File nicht gefunden wird</exception>
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

        /// <summary>
        /// Methode welche die logs aus der ReadLog Methode in eine Liste von LogViewModels umwandelt und parsed
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
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


        private static (string Salt, string HashedPassword) HashPasswordWithSalt(byte[] salt, string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                byte[] hashBytes = sha256.ComputeHash(saltedPassword);
                return (Convert.ToBase64String(salt), Convert.ToBase64String(hashBytes));
            }
        }
    }
}