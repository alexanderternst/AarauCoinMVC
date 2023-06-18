using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AarauCoinMVC.Controllers
{
    public class AccountController : Controller
    {

        public IActionResult IdleTimeout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IdleTimeout(string stayLoggedIn)
        {
            if (Convert.ToBoolean(stayLoggedIn))
            {
                // Reset the last activity timestamp and allow the user to stay logged in

                //HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString());
                return RedirectToAction("Index", "Home"); // Redirect to a suitable page
            }
            else
            {
                HttpContext.SignOutAsync("AarauCoin-AuthenticationScheme");
                return RedirectToAction("Index", "Home");

            }
        }
    }
}
