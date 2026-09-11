using Microsoft.AspNetCore.Identity;
using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobsMvc.Data;

public static class DbSeeder
{
    public static async Task SeedDefaultDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        

        string[] roles = ["Admin", "Company", "JobSeeker"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

       

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

       

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category
                {
                    Name = "Software Development",
                    IconClass = "fa-code"
                },

                new Category
                {
                    Name = "Design & Creative",
                    IconClass = "fa-drafting-compass"
                },

                new Category
                {
                    Name = "Marketing & Sales",
                    IconClass = "fa-chart-line"
                },

                new Category
                {
                    Name = "Customer Service",
                    IconClass = "fa-headset"
                },

                new Category
                {
                    Name = "Human Resources",
                    IconClass = "fa-user-tie"
                }
            );

            await context.SaveChangesAsync();
        }

       

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

        

        var demoCompanyUser =
            await userManager.FindByEmailAsync("demo-company@test.com");

        if (demoCompanyUser == null)
        {
            demoCompanyUser = new ApplicationUser
            {
                UserName = "demo-company@test.com",
                Email = "demo-company@test.com",
                EmailConfirmed = true,
                UserType = UserType.Company
            };

            var result = await userManager.CreateAsync(
                demoCompanyUser,
                "Demo@123"
            );

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(
                    demoCompanyUser,
                    "Company"
                );

                var companyProfile = new CompanyProfile
                {
                    Id = demoCompanyUser.Id,
                    CompanyName = "Demo Company",
                    Industry = "Software Development",
                    IsApproved = true
                };

                context.CompanyProfiles.Add(companyProfile);

                await context.SaveChangesAsync();

                // Add Cairo as an operating city
                var cairoCity = await context.Cities
                    .FirstOrDefaultAsync(c => c.Name == "Cairo");

                if (cairoCity != null)
                {
                    context.CompanyProfileCities.Add(
                        new CompanyProfile_City
                        {
                            CompanyProfileId = companyProfile.Id,
                            CityId = cairoCity.Id
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }

        

        var company = await context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.CompanyName == "Demo Company");

        if (company != null)
        {
            var companyHasJobs = await context.JobPosts
                .AnyAsync(j => j.CompanyId == company.Id);

            if (!companyHasJobs)
            {
               

                var cairo = await context.Cities
                    .FirstAsync(c => c.Name == "Cairo");

                var giza = await context.Cities
                    .FirstAsync(c => c.Name == "Giza");

                var alexandria = await context.Cities
                    .FirstAsync(c => c.Name == "Alexandria");

                var fayoum = await context.Cities
                    .FirstAsync(c => c.Name == "Fayoum");

               

                var softwareCategory = await context.Categories
                    .FirstAsync(c => c.Name == "Software Development");

                var designCategory = await context.Categories
                    .FirstAsync(c => c.Name == "Design & Creative");

                var marketingCategory = await context.Categories
                    .FirstAsync(c => c.Name == "Marketing & Sales");

                

                var csharp = await context.Skills
                    .FirstAsync(s => s.Name == "C#");

                var dotnet = await context.Skills
                    .FirstAsync(s => s.Name == ".NET Core");

                var aspnet = await context.Skills
                    .FirstAsync(s => s.Name == "ASP.NET MVC");

                var sql = await context.Skills
                    .FirstAsync(s => s.Name == "SQL Server");

                var javascript = await context.Skills
                    .FirstAsync(s => s.Name == "JavaScript");

                var react = await context.Skills
                    .FirstAsync(s => s.Name == "React");

                var problemSolving = await context.Skills
                    .FirstAsync(s => s.Name == "Problem Solving");

                var git = await context.Skills
                    .FirstAsync(s => s.Name == "Git & GitHub");

               

                var job1 = new JobPost
                {
                    Title = "Junior .NET Developer",
                    Description =
                        "We are looking for a Junior .NET Developer to join our software development team.",
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.Junior,
                    MinSalary = 12000,
                    MaxSalary = 18000,
                    ClosingDate = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CityId = cairo.Id,
                    CompanyId = company.Id,
                    CategoryId = softwareCategory.Id
                };

                job1.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = csharp.Id
                    }
                );

                job1.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = dotnet.Id
                    }
                );

                job1.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = sql.Id
                    }
                );

                context.JobPosts.Add(job1);

               

                var job2 = new JobPost
                {
                    Title = "ASP.NET MVC Developer",
                    Description =
                        "Join our backend team and help us build modern web applications using ASP.NET MVC.",
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.MidLevel,
                    MinSalary = 18000,
                    MaxSalary = 28000,
                    ClosingDate = DateTime.UtcNow.AddDays(25),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    CityId = giza.Id,
                    CompanyId = company.Id,
                    CategoryId = softwareCategory.Id
                };

                job2.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = csharp.Id
                    }
                );

                job2.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = aspnet.Id
                    }
                );

                job2.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = sql.Id
                    }
                );

                job2.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = git.Id
                    }
                );

                context.JobPosts.Add(job2);

                

                var job3 = new JobPost
                {
                    Title = "Frontend Developer",
                    Description =
                        "We are looking for a Frontend Developer to create responsive and user-friendly web interfaces.",
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.Junior,
                    MinSalary = 10000,
                    MaxSalary = 17000,
                    ClosingDate = DateTime.UtcNow.AddDays(20),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CityId = cairo.Id,
                    CompanyId = company.Id,
                    CategoryId = softwareCategory.Id
                };

                job3.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = javascript.Id
                    }
                );

                job3.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = react.Id
                    }
                );

                job3.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = git.Id
                    }
                );

                context.JobPosts.Add(job3);

               

                var job4 = new JobPost
                {
                    Title = "Senior .NET Developer",
                    Description =
                        "We are seeking an experienced Senior .NET Developer to lead backend development.",
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.Senior,
                    MinSalary = 30000,
                    MaxSalary = 45000,
                    ClosingDate = DateTime.UtcNow.AddDays(35),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    CityId = alexandria.Id,
                    CompanyId = company.Id,
                    CategoryId = softwareCategory.Id
                };

                job4.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = csharp.Id
                    }
                );

                job4.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = dotnet.Id
                    }
                );

                job4.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = sql.Id
                    }
                );

                job4.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = problemSolving.Id
                    }
                );

                context.JobPosts.Add(job4);

                

                var job5 = new JobPost
                {
                    Title = "React Developer",
                    Description =
                        "We are looking for a React Developer to build modern frontend applications.",
                    JobType = JobType.Remote,
                    ExperienceLevel = ExperienceLevel.MidLevel,
                    MinSalary = 20000,
                    MaxSalary = 30000,
                    ClosingDate = DateTime.UtcNow.AddDays(28),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    CityId = cairo.Id,
                    CompanyId = company.Id,
                    CategoryId = softwareCategory.Id
                };

                job5.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = javascript.Id
                    }
                );

                job5.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = react.Id
                    }
                );

                job5.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = git.Id
                    }
                );

                context.JobPosts.Add(job5);

               

                var job6 = new JobPost
                {
                    Title = "UI/UX Designer",
                    Description =
                        "We are looking for a creative UI/UX Designer to design simple and engaging user experiences.",
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.Junior,
                    MinSalary = 9000,
                    MaxSalary = 15000,
                    ClosingDate = DateTime.UtcNow.AddDays(18),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CityId = giza.Id,
                    CompanyId = company.Id,
                    CategoryId = designCategory.Id
                };

                job6.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = problemSolving.Id
                    }
                );

                context.JobPosts.Add(job6);

               

                var job7 = new JobPost
                {
                    Title = "Digital Marketing Specialist",
                    Description =
                        "We are looking for a Digital Marketing Specialist to manage online campaigns and marketing activities.",
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.MidLevel,
                    MinSalary = 12000,
                    MaxSalary = 20000,
                    ClosingDate = DateTime.UtcNow.AddDays(22),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    CityId = fayoum.Id,
                    CompanyId = company.Id,
                    CategoryId = marketingCategory.Id
                };

                job7.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = problemSolving.Id
                    }
                );

                context.JobPosts.Add(job7);

                

                var job8 = new JobPost
                {
                    Title = "Software Development Intern",
                    Description =
                        "An internship opportunity for fresh graduates who want to start their career in software development.",
                    JobType = JobType.Internship,
                    ExperienceLevel = ExperienceLevel.FreshGrad,
                    MinSalary = 5000,
                    MaxSalary = 7000,
                    ClosingDate = DateTime.UtcNow.AddDays(15),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    CityId = cairo.Id,
                    CompanyId = company.Id,
                    CategoryId = softwareCategory.Id
                };

                job8.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = csharp.Id
                    }
                );

                job8.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = problemSolving.Id
                    }
                );

                context.JobPosts.Add(job8);

                await context.SaveChangesAsync();
            }
        }
    }
}

