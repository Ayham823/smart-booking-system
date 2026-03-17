using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartBookingSystem.Models;

namespace SmartBookingSystem.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            string[] roles = { "Admin", "Coach", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@smartbooking.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "System Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var coachEmail = "coach1@smartbooking.com";
            var coachUser = await userManager.FindByEmailAsync(coachEmail);

            if (coachUser == null)
            {
                coachUser = new ApplicationUser
                {
                    FullName = "Coach One",
                    UserName = coachEmail,
                    Email = coachEmail,
                    EmailConfirmed = true
                };

                var coachResult = await userManager.CreateAsync(coachUser, "Coach123!");

                if (coachResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(coachUser, "Coach");
                }
            }

            if (coachUser != null && !context.Coaches.Any(c => c.UserId == coachUser.Id))
            {
                context.Coaches.Add(new Coach
                {
                    UserId = coachUser.Id,
                    Specialty = "ASP.NET Core",
                    Bio = "Experienced ASP.NET Core coach",
                    IsActive = true
                });

                await context.SaveChangesAsync();
            }

            if (!context.TrainingServices.Any())
            {
                context.TrainingServices.AddRange(
                    new TrainingService
                    {
                        Name = "ASP.NET Core Basics",
                        Description = "Introduction to ASP.NET Core MVC",
                        DurationInMinutes = 90,
                        Price = 150,
                        IsActive = true
                    },
                    new TrainingService
                    {
                        Name = "Entity Framework Core",
                        Description = "Learn EF Core and database operations",
                        DurationInMinutes = 120,
                        Price = 200,
                        IsActive = true
                    },
                    new TrainingService
                    {
                        Name = "JavaScript and Ajax",
                        Description = "Dynamic client-side interaction",
                        DurationInMinutes = 100,
                        Price = 180,
                        IsActive = true
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}