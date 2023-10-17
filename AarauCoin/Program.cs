using AarauCoin.Database;
using AarauCoin.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AarauCoin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "AarauCoin-AuthenticationScheme";
                options.DefaultSignInScheme = "AarauCoin-AuthenticationScheme";
                options.DefaultChallengeScheme = "AarauCoin-AuthenticationScheme";
            })
                .AddCookie("AarauCoin-AuthenticationScheme", options =>
                {
                    // Configure the authentication cookie options
                    options.Cookie.Name = "AarauCoin-AuthenticationCookie";
                    options.Cookie.HttpOnly = true;
                    options.SlidingExpiration = true;
                });
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<AarauCoinContext>(options => options.UseInMemoryDatabase(databaseName: "AarauCoinDb"));

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            app.Run();
        }
    }
}