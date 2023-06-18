using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace AarauCoinMVC.Middleware
{
    public class TimeoutMiddleware
    {
        private readonly RequestDelegate _next;

        public TimeoutMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var lastActivity = context.Session.GetString("LastActivity");
            var idleTimeoutString = context.Session.GetString("IdleTimeout");

            if (!string.IsNullOrEmpty(lastActivity) && TimeSpan.TryParse(idleTimeoutString, out var idleTimeout))
            {
                var lastActivityTime = DateTime.Parse(lastActivity);
                var currentTime = DateTime.UtcNow;

                if (currentTime - lastActivityTime > idleTimeout)
                {
                    // User has exceeded the idle timeout, redirect to a page to confirm stay logged in or log out
                    context.Response.Redirect("/Account/IdleTimeout");

                    return;
                }
            }
            await _next(context);
        }
    }
}
