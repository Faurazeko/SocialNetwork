using Microsoft.EntityFrameworkCore;
using SocialNetwork.Models;

namespace SocialNetwork.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProduction)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProduction);
            }
        }

        private static void SeedData(AppDbContext context, bool isProduction)
        {

            if (isProduction)
            {
                Console.WriteLine("--> Attempting to apply migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migration: {ex.Message}");
                }
            }

            if (!context.Users.Any())
            {
                Console.WriteLine("--> Seeding data...");

                context.Users.AddRange(new[]
                {
                    new User() 
                    { 
                        Nickname = "Faurazeko", Password = "Faurazeko", IsAdmin = true, IsBlocked = false, CreatedDateTime = DateTime.Now, AboutText = "",
                        IsOnline = false, LastOnlineTime = DateTime.MinValue, Trollars = 0
                    },
                    new User() 
                    { 
                        Nickname = "User", Password = "User", IsAdmin = false, IsBlocked = false, CreatedDateTime = DateTime.Now, AboutText = "",
                        IsOnline = false, LastOnlineTime = DateTime.MinValue, Trollars = 0
                    },
                    new User() 
                    { 
                        Nickname = "Blocked", Password = "Blocked", IsAdmin = false, IsBlocked = true, CreatedDateTime = DateTime.Now, AboutText = "", 
                        IsOnline = false, LastOnlineTime = DateTime.MinValue, Trollars = 0
                    },
                });

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> We already have data");
            }
        }
    }
}
