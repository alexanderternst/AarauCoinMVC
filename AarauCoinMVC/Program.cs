using AarauCoinMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace AarauCoinMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // TODO: Improve with better names
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "YourAuthenticationScheme";
                    options.DefaultSignInScheme = "YourAuthenticationScheme";
                    options.DefaultChallengeScheme = "YourAuthenticationScheme";
                })
                .AddCookie("YourAuthenticationScheme", options =>
                {
                    // Configure the authentication cookie options
                    options.Cookie.Name = "YourAuthenticationCookie";
                    options.Cookie.HttpOnly = true;
                    options.SlidingExpiration = true;
                });

            builder.Services.AddDbContext<AarauCoinContext>(options => options.UseInMemoryDatabase(databaseName: "AuthorDb"));
            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}