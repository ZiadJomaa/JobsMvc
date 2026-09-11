using Microsoft.AspNetCore.Identity;
using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;

namespace JobsMvc.Data;

public static class DbSeeder
{
    public static async Task SeedDefaultDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Seed Roles
        string[] roles = ["Admin", "Company", "JobSeeker"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Cities
        if (!context.Cities.Any())
        {
            context.Cities.AddRange(
                new City { Name = "Cairo" },
                new City { Name = "Giza" },
                new City { Name = "Alexandria" },
                new City { Name = "Fayoum" },
                new City { Name = "Mansoura" }
            );
            await context.SaveChangesAsync();
        }

        // 3. Seed Categories
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Software Development", IconClass = "fa-code" },
                new Category { Name = "Design & Creative", IconClass = "fa-drafting-compass" },
                new Category { Name = "Marketing & Sales", IconClass = "fa-chart-line" },
                new Category { Name = "Customer Service", IconClass = "fa-headset" },
                new Category { Name = "Human Resources", IconClass = "fa-user-tie" }
            );
            await context.SaveChangesAsync();
        }

        // 4. Seed Skills
        if (!context.Skills.Any())
        {
            context.Skills.AddRange(
                new Skill { Name = "C#" },
                new Skill { Name = ".NET Core" },
                new Skill { Name = "ASP.NET MVC" },
                new Skill { Name = "SQL Server" },
                new Skill { Name = "JavaScript" },
                new Skill { Name = "React" },
                new Skill { Name = "Problem Solving" },
                new Skill { Name = "Git & GitHub" }
            );
            await context.SaveChangesAsync();
        }
    }
}