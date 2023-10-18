using AarauCoin.Database;
using AarauCoin.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AarauCoin.Services
{
    public class UserService
    {
        private readonly AarauCoinContext _context;

        public UserService(AarauCoinContext context)
        {
            _context = context;
        }

        public async Task InsertUser()
        {
            IQueryable<User> user = _context.Users;
            IQueryable<Level> lev = _context.Levels;
            if (!user.Any() && !lev.Any())
            {
                _context.Levels.Add(new Level
                {
                    Id = 1,
                    Name = "Admin"
                });
                _context.SaveChanges();

                _context.Levels.Add(new Level
                {
                    Id = 2,
                    Name = "User"
                });
                _context.SaveChanges();

                await CreateUser("Hans", "ibz1234", "User", 1000);

                await CreateUser("Alex", "ibz1234", "Admin", 1000);
            }
        }


        #region Get user data

        public async Task<User?> Login(string username)
        {
            return await _context.Users.Include(x => x.Level).FirstOrDefaultAsync(s => s.Username.ToLower() == username.ToLower()); // returns new user object when no user found
        }

        public bool VerifyPassword(string storedSalt, string storedHashedPassword, string attemptedPassword)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            var (_, HashedPassword) = HashPasswordWithSalt(salt, attemptedPassword);            
            return HashedPassword == storedHashedPassword; 
        }

        /// <summary>
        /// Methode für das Abrufen aller Benutzernamen
        /// </summary>
        /// <returns></returns>
        public IQueryable<string> GetUserNames()
        {
            return _context.Users.Select(s => s.Username);
        }

        /// <summary>
        /// Methode für das Abrufen der Benutzerdaten mit den wichtigsten Informationen
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<UserAccountViewModel> GetUserInformation(string username)
        {
            var user = await _context.Users.
                  Select(e => new UserAccountViewModel
                  {
                      Username = e.Username,
                      Level = e.Level.Name,
                      Coins = _context.CoinAccounts.Where(s => s.User.Username == username).FirstOrDefault(),
                  }).FirstAsync(s => s.Username.ToLower() == username.ToLower());
            return user;
        }

        /// <summary>
        /// Methode für das Abrufen aller Benutzernamen und deren Coins
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UserWithCoin>> GetAllUsers()
        {
            return await _context.Users.Select(e => new UserWithCoin
            {
                Username = e.Username,
                Coins = _context.CoinAccounts.Where(s => s.User.Username == e.Username).First().Coins
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
                throw new Exception($"User with username {username} already exists");

            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var (Salt, HashedPassword) = HashPasswordWithSalt(salt, password);

            await _context.Users.AddAsync(
                new User
                {
                    Username = username,
                    Password = HashedPassword,
                    Salt = Salt,
                    HashingAlgorithm = "SHA256",
                    Level = _context.Levels.First(s => s.Name == level)
                });
            await _context.SaveChangesAsync();

            await _context.CoinAccounts.AddAsync(
                 new CoinAccount
                 {
                     Coins = coins,
                     User = _context.Users.First(s => s.Username == username)
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
            var reciever = await _context.CoinAccounts.FirstOrDefaultAsync(s => s.User.Username == username);
            if (reciever == null)
                throw new UserException("User has no account");

            _context.CoinAccounts.Where(s => s.User.Username == username).First().Coins = coins;
            await _context.SaveChangesAsync();
        }

        #endregion Set user data

        private static (string Salt, string HashedPassword) HashPasswordWithSalt(byte[] salt, string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                byte[] hashBytes = sha256.ComputeHash(saltedPassword);
                return (Convert.ToBase64String(salt), Convert.ToBase64String(hashBytes));
            }
        }

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
            var senderAccount = await _context.CoinAccounts.FirstOrDefaultAsync(s => s.User.Username == sender);
            var receiverAccount = await _context.CoinAccounts.FirstOrDefaultAsync(s => s.User.Username == receiver);

            if (senderAccount == null)
                throw new UserException("Sender has no account");

            if (receiverAccount == null)
                throw new UserException("Receiver has no account");

            if (senderAccount.Coins < amount)
                throw new UserException("Not enough coins in account");

            senderAccount.Coins -= amount;
            receiverAccount.Coins += amount;

            await _context.SaveChangesAsync();
        }
    }
}
