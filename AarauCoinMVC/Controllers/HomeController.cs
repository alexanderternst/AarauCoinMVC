using AarauCoinMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace AarauCoinMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AarauCoinContext _context;

        public HomeController(ILogger<HomeController> logger, AarauCoinContext context)
        {
            _context = context;
            _logger = logger;

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

        public IActionResult Index()
        {
            return View("Index");
        }

        //[Authorize] -- Does not work yet probably needs more arguments
        public IActionResult Privacy()
        {
            // Knows to return Privacy View because of the name of the method
            // This works because HomeController inherits from Controller
            // We can also specify the name of the View to return like we did in Index()
            return View();
        }

        public IActionResult Login()
        {
            // open login page
            // we can either do if statement here or in cshtml (to check if user is logged in and return appropriate view)
            return View();
        }

        public IActionResult LoginAction(LoginModel loginData)
        {
            UserLoginDTO list = _context.Users.
                    Select(e => new UserLoginDTO
                    {
                        Id = e.UserId,
                        Username = e.Username,
                        Password = e.Password,
                        Level = e.LevelId.LevelName
                    }).First(s => s.Username == loginData.Username);

            if (loginData.Username == list.Username && loginData.Password == list.Password)
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginData.Username), // Add claims as needed
                    new Claim (ClaimTypes.Role, list.Level) // Add claims as needed
                };

                var claimsIdentity = new ClaimsIdentity(claims, "AarauCoin-AuthenticationScheme");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Set whether the cookie should persist across sessions
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(1) // Set the expiration time of the cookie
                };

                HttpContext.SignInAsync("AarauCoin-AuthenticationScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
                
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        public IActionResult LogoutAction(string username)
        {
            //var a = HttpContext.User.Claims.ToList();
            //var c = HttpContext.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Role);
            //var b = User.FindFirst(ClaimTypes.Role).Value.ToString();

            HttpContext.SignOutAsync("YourAuthenticationScheme");
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}