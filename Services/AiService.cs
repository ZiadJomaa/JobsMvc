using System.Text.Json;
using JobsMvc.Data;
using Microsoft.Extensions.Configuration;

namespace JobsMvc.Services
{
    public class AiService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AiService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> GetAnswerFromAiAsync(string userQuestion)
        {
            if (string.IsNullOrEmpty(userQuestion))
            {
                return JsonSerializer.Serialize(new { type = "text", content = "Please enter a valid question." });
            }

            string q = userQuestion.ToLower().Trim();

            // Handle general conversation, greetings, and appreciation gracefully
            if (q == "hi" || q == "hello" || q == "hey")
            {
                return JsonSerializer.Serialize(new { type = "text", content = "Hello! I am your Job Assistant. How can I help you find your next career opportunity today?" });
            }

            if (q.Contains("how are you"))
            {
                return JsonSerializer.Serialize(new { type = "text", content = "I'm doing great, thank you for asking! I'm ready to help you explore available job listings and career opportunities. What are you looking for?" });
            }

            if (q.Contains("thank"))
            {
                return JsonSerializer.Serialize(new { type = "text", content = "You're very welcome! If you need help finding any other jobs or positions, just let me know." });
            }

            if (q.Contains("who are you") || q.Contains("what can you do"))
            {
                return JsonSerializer.Serialize(new { type = "text", content = "I am a local AI assistant integrated into the Job Portal. I can help you search for available jobs, filter positions by technologies like .NET or C#, or find junior and intern roles." });
            }

            var jobs = _context.JobPosts.ToList();

            if (jobs == null || !jobs.Any())
            {
                return JsonSerializer.Serialize(new { type = "text", content = "Sorry, there are currently no jobs available in the database." });
            }

            // Determine if the user is asking specifically about filtered types (like junior, .net, etc.)
            var targetJobs = jobs;
            bool isJuniorQuery = q.Contains("junior") || q.Contains("intern");
            bool isNetQuery = q.Contains("net") || q.Contains("c#") || q.Contains("developer");

            if (isJuniorQuery)
            {
                targetJobs = jobs.Where(j =>
                    (!string.IsNullOrEmpty(j.Title) && (j.Title.ToLower().Contains("junior") || j.Title.ToLower().Contains("intern"))) ||
                    (!string.IsNullOrEmpty(j.Description) && (j.Description.ToLower().Contains("junior") || j.Description.ToLower().Contains("intern")))
                ).ToList();
            }
            else if (isNetQuery)
            {
                targetJobs = jobs.Where(j =>
                    (!string.IsNullOrEmpty(j.Title) && (j.Title.ToLower().Contains("net") || j.Title.ToLower().Contains("c#") || j.Title.ToLower().Contains("developer"))) ||
                    (!string.IsNullOrEmpty(j.Description) && (j.Description.ToLower().Contains("net") || j.Description.ToLower().Contains("c#") || j.Description.ToLower().Contains("developer")))
                ).ToList();
            }

            // Handle Salary questions intelligently based on targeted or all jobs
            if (q.Contains("salary") || q.Contains("salaries") || q.Contains("pay") || q.Contains("money"))
            {
                if (!targetJobs.Any())
                {
                    return JsonSerializer.Serialize(new { type = "text", content = "Sorry, no matching jobs were found to display salaries for." });
                }

                var salaryDetails = targetJobs.Select(j => {
                    string salaryText = "Not Specified";
                    if (j.MinSalary.HasValue && j.MaxSalary.HasValue)
                    {
                        salaryText = $"{j.MinSalary} to {j.MaxSalary}";
                    }
                    else if (j.MinSalary.HasValue)
                    {
                        salaryText = $"From {j.MinSalary}";
                    }
                    else if (j.MaxSalary.HasValue)
                    {
                        salaryText = $"Up to {j.MaxSalary}";
                    }
                    return $"- {j.Title}: {salaryText}";
                }).ToList();

                string responsePrefix = isJuniorQuery ? "Here are the salary details for junior positions:\n" :
                                        isNetQuery ? "Here are the salary details for .NET roles:\n" :
                                        "Here are the salary details for the available positions:\n";

                string salaryResponse = responsePrefix + string.Join("\n", salaryDetails);

                return JsonSerializer.Serialize(new { type = "text", content = salaryResponse });
            }

            var filteredJobs = targetJobs;

            if (!string.IsNullOrEmpty(userQuestion))
            {
                if (isNetQuery || isJuniorQuery)
                {
                    // Already filtered above
                }
                else if (q.Contains("all") || q.Contains("available") || q.Contains("jobs"))
                {
                    filteredJobs = jobs;
                }
                else
                {
                    return JsonSerializer.Serialize(new { type = "text", content = $"I understand you are asking about \"{userQuestion}\". Currently, I can best assist you with finding available jobs, .NET positions, or junior roles in our database!" });
                }
            }

            if (filteredJobs == null || !filteredJobs.Any())
            {
                return JsonSerializer.Serialize(new { type = "text", content = "Sorry, no matching jobs were found based on your request." });
            }

            // Return structured jobs list for cards
            var jobListResponse = filteredJobs.Select(j => new {
                title = j.Title,
                description = j.Description
            }).ToList();

            return JsonSerializer.Serialize(new { type = "jobs", content = jobListResponse });
        }
    }
}