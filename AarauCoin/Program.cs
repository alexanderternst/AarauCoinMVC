using AarauCoin.Database;
using AarauCoin.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;
using System.Configuration;
using static System.Net.WebRequestMethods;

namespace AarauCoin
{
    public class Program
    {
        // Tools > NuGet Package Manager > Package Manager Console
        // --- Datenbank erstellen ---
        // Add-Migration InitialCreate
        // Update-Database

        // --- Datenbank updaten ---
        // Add-Migration LimitStrings
        // Update-Database

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

            // In memory
            builder.Services.AddDbContext<AarauCoinContext>(options => options.UseInMemoryDatabase(databaseName: "AarauCoinDb"));

            // MySql (MariaDb)

            // Open issue with pomelo, known issue (MariaDb) not working
            // https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1722
            // https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1714
            // https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1548
            // Issue is sub query with OR filter in main query. MariaDb does not support "lateral derived tables" which causes this issue
            // https://jira.mariadb.org/browse/MDEV-19078

            //var connectionString = builder.Configuration.GetConnectionString("AarauCoinDb") ?? "";
            //var serverVersion = ServerVersion.AutoDetect(connectionString);
            //// Replace 'YourDbContext' with the name of your own DbContext derived class.
            //builder.Services.AddDbContext<AarauCoinContext>(
            //    dbContextOptions => dbContextOptions
            //        .UseMySql(connectionString, serverVersion)
            //        // The following three options help with debugging, but should
            //        // be changed or removed for production.
            //        .LogTo(Console.WriteLine, LogLevel.Information)
            //        .EnableSensitiveDataLogging()
            //        .EnableDetailedErrors()
            //);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<LogEntryService>();
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

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