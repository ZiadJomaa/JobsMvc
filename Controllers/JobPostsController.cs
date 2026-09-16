using JobsMvc.Data;
using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;
using JobsMvc.ViewModels.JobPost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace JobsMvc.Controllers
{
    public class JobPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; 

        public JobPostsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ============================================================
        // INDEX
        // ============================================================

        public async Task<IActionResult> Index(
            string searchTitle,
            int? cityId,
            int? categoryId,
            JobType? jobType,
            ExperienceLevel? experienceLevel,
            decimal? minSalary,
            decimal? maxSalary,
            string sortOrder,
            int page = 1)
        {
            if (page < 1)
                page = 1;

            var query = _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.City)
                .Include(j => j.Category)
                .Where(j =>
                    !j.ClosingDate.HasValue ||
                    j.ClosingDate.Value >= DateTime.UtcNow)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                query = query.Where(j =>
                    j.Title.Contains(searchTitle));
            }

            if (cityId.HasValue)
            {
                query = query.Where(j =>
                    j.CityId == cityId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(j =>
                    j.CategoryId == categoryId.Value);
            }

            if (jobType.HasValue)
            {
                query = query.Where(j =>
                    j.JobType == jobType.Value);
            }

            if (experienceLevel.HasValue)
            {
                query = query.Where(j =>
                    j.ExperienceLevel == experienceLevel.Value);
            }

            if (minSalary.HasValue)
            {
                query = query.Where(j =>
                    j.MinSalary >= minSalary.Value);
            }

            if (maxSalary.HasValue)
            {
                query = query.Where(j =>
                    j.MaxSalary <= maxSalary.Value);
            }

            switch (sortOrder)
            {
                case "oldest":
                    query = query.OrderBy(j => j.CreatedAt);
                    break;

                case "salaryHigh":
                    query = query.OrderByDescending(j => j.MaxSalary);
                    break;

                case "salaryLow":
                    query = query.OrderBy(j => j.MinSalary);
                    break;

                default:
                    query = query.OrderByDescending(j => j.CreatedAt);
                    break;
            }

            const int pageSize = 6;

            var totalJobs = await query.CountAsync();

            var totalPages =
                (int)Math.Ceiling(
                    (double)totalJobs / pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var jobPosts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SortOrder = sortOrder;

            ViewBag.Cities = new SelectList(
                await _context.Cities
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                cityId);

            ViewBag.Categories = new SelectList(
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                categoryId);

            return View(jobPosts);
        }

        // ============================================================
        // DETAILS
        // ============================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var jobPost = await _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.City)
                .Include(j => j.Category)
                .Include(j => j.RequiredSkills)
                    .ThenInclude(rs => rs.Skill)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (jobPost == null)
                return NotFound();

            return View(jobPost);
        }

        // ============================================================
        // CREATE - GET
        // ============================================================

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create()
        {
            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(
                        c => c.Id == companyId);

            if (company == null)
                return NotFound();

            if (!company.IsApproved)
                return Forbid();

            var vm = new JobPostFormViewModel();

            await PopulateDropdownsAsync(vm);

            return View(vm);
        }

        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create(
            JobPostFormViewModel vm)
        {
            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(
                        c => c.Id == companyId);

            if (company == null)
                return NotFound();

            if (!company.IsApproved)
                return Forbid();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            var jobPost = new JobPost
            {
                Title = vm.Title,
                Description = vm.Description,
                JobType = vm.JobType,
                ExperienceLevel = vm.ExperienceLevel,
                MinSalary = vm.MinSalary,
                MaxSalary = vm.MaxSalary,
                ClosingDate = vm.ClosingDate,
                CityId = vm.CityId,
                CategoryId = vm.CategoryId,
                CompanyId = companyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var skillId in vm.SelectedSkillIds)
            {
                jobPost.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        SkillId = skillId
                    });
            }

            _context.JobPosts.Add(jobPost);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Job created successfully!";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // EDIT - GET
        // ============================================================

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost =
                await _context.JobPosts
                    .Include(j => j.RequiredSkills)
                    .FirstOrDefaultAsync(j =>
                        j.Id == id &&
                        j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            var vm = new JobPostFormViewModel
            {
                Id = jobPost.Id,
                Title = jobPost.Title,
                Description = jobPost.Description,
                JobType = jobPost.JobType,
                ExperienceLevel = jobPost.ExperienceLevel,
                MinSalary = jobPost.MinSalary,
                MaxSalary = jobPost.MaxSalary,
                ClosingDate = jobPost.ClosingDate,
                CityId = jobPost.CityId,
                CategoryId = jobPost.CategoryId,

                SelectedSkillIds =
                    jobPost.RequiredSkills
                        .Select(rs => rs.SkillId)
                        .ToList()
            };

            await PopulateDropdownsAsync(vm);

            return View(vm);
        }

        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(
            int id,
            JobPostFormViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost =
                await _context.JobPosts
                    .Include(j => j.RequiredSkills)
                    .FirstOrDefaultAsync(j =>
                        j.Id == id &&
                        j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(vm);
                return View(vm);
            }

            jobPost.Title = vm.Title;
            jobPost.Description = vm.Description;
            jobPost.JobType = vm.JobType;
            jobPost.ExperienceLevel = vm.ExperienceLevel;
            jobPost.MinSalary = vm.MinSalary;
            jobPost.MaxSalary = vm.MaxSalary;
            jobPost.ClosingDate = vm.ClosingDate;
            jobPost.CityId = vm.CityId;
            jobPost.CategoryId = vm.CategoryId;

            jobPost.RequiredSkills.Clear();

            foreach (var skillId in vm.SelectedSkillIds)
            {
                jobPost.RequiredSkills.Add(
                    new JobPost_Skill
                    {
                        JobPostId = jobPost.Id,
                        SkillId = skillId
                    });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Job updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETE - GET
        // ============================================================

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost =
                await _context.JobPosts
                    .Include(j => j.Company)
                    .Include(j => j.City)
                    .Include(j => j.Category)
                    .FirstOrDefaultAsync(j =>
                        j.Id == id &&
                        j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            return View(jobPost);
        }

        // ============================================================
        // DELETE - POST
        // ============================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost =
                await _context.JobPosts
                    .FirstOrDefaultAsync(j =>
                        j.Id == id &&
                        j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            _context.JobPosts.Remove(jobPost);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Job deleted successfully!";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // CLOSE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Close(int id)
        {
            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost =
                await _context.JobPosts
                    .FirstOrDefaultAsync(j =>
                        j.Id == id &&
                        j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            jobPost.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Job closed successfully!";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // APPLY
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Apply(
            int jobPostId)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var jobPost =
                await _context.JobPosts
                    .FirstOrDefaultAsync(
                        j => j.Id == jobPostId);

            if (jobPost == null)
                return NotFound();

            if (!jobPost.IsActive)
            {
                TempData["Error"] =
                    "This job is no longer active.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = jobPostId });
            }

            var jobSeekerProfile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(
                        s => s.Id == userId);

            if (jobSeekerProfile == null)
            {
                jobSeekerProfile =
                    new JobSeekerProfile
                    {
                        Id = userId
                    };

                _context.JobSeekerProfiles.Add(
                    jobSeekerProfile);

                await _context.SaveChangesAsync();
            }

            var existingApplication =
                await _context.JobApplications
                    .FirstOrDefaultAsync(a =>
                        a.JobPostId == jobPostId &&
                        a.JobSeekerId == userId);

            if (existingApplication != null)
            {
                TempData["Error"] =
                    "You have already applied for this job.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = jobPostId });
            }

            var application =
                new JobApplication
                {
                    JobPostId = jobPostId,
                    JobSeekerId = userId,
                    AppliedAt = DateTime.UtcNow,
                    Status = ApplicationStatus.Applied
                };

            _context.JobApplications.Add(application);

            await _context.SaveChangesAsync();

            var history =
                new ApplicationStatusHistory
                {
                    JobApplicationId = application.Id,
                    Status = ApplicationStatus.Applied,
                    ChangedAt = application.AppliedAt
                };

            _context.ApplicationStatusHistories.Add(
                history);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your application has been submitted successfully!";

            return RedirectToAction(
                nameof(Details),
                new { id = jobPostId });
        }

        // ============================================================
        // DROPDOWNS
        // ============================================================

        private async Task PopulateDropdownsAsync(
            JobPostFormViewModel vm)
        {
            vm.Cities = new SelectList(
                await _context.Cities
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                vm.CityId);

            vm.Categories = new SelectList(
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name",
                vm.CategoryId);

            vm.AllSkills =
                await _context.Skills
                    .OrderBy(s => s.Name)
                    .ToListAsync();
        }

        // ============================================================
        // VIEW APPLICATIONS
        // ============================================================

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> ViewApplications(
            int id,
            ApplicationStatus? status)
        {
            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var jobPost =
                await _context.JobPosts
                    .Include(j => j.Applications)
                        .ThenInclude(a => a.JobSeeker)
                            .ThenInclude(s => s.Resumes)
                    .FirstOrDefaultAsync(j =>
                        j.Id == id &&
                        j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            var applications =
                jobPost.Applications.AsQueryable();

            if (status.HasValue)
            {
                applications =
                    applications.Where(
                        a => a.Status == status.Value);
            }

            ViewBag.SelectedStatus = status;

            jobPost.Applications =
                applications
                    .OrderByDescending(a => a.AppliedAt)
                    .ToList();

            return View(jobPost);
        }

        // ============================================================
        // UPDATE APPLICATION STATUS
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateApplicationStatus(
            int applicationId,
            ApplicationStatus status,
            string? privateNote)
        {
            var application =
                await _context.JobApplications
                    .Include(a => a.JobPost)
                    .FirstOrDefaultAsync(
                        a => a.Id == applicationId);

            if (application == null)
                return NotFound();

            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (application.JobPost.CompanyId != companyId)
                return Forbid();

            application.Status = status;

            if (!string.IsNullOrWhiteSpace(privateNote))
            {
                application.HrPrivateNote =
                    privateNote;
            }

            await _context.SaveChangesAsync();

            var history =
                new ApplicationStatusHistory
                {
                    JobApplicationId =
                        application.Id,

                    Status = status,

                    ChangedAt =
                        DateTime.UtcNow
                };

            _context.ApplicationStatusHistories.Add(
                history);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Application status updated successfully!";

            return RedirectToAction(
                nameof(ViewApplications),
                new { id = application.JobPostId });
        }

        // ============================================================
        // COMPANY DASHBOARD
        // ============================================================

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> CompanyDashboard()
        {
            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobs =
                await _context.JobPosts
                    .Where(j =>
                        j.CompanyId == companyId)
                    .Include(j => j.Applications)
                        .ThenInclude(a => a.JobSeeker)
                    .ToListAsync();

            var applications =
                jobs
                    .SelectMany(j => j.Applications)
                    .ToList();

            var weekStart =
                DateTime.UtcNow.AddDays(-7);

            ViewBag.OpenJobs =
                jobs.Count(j =>
                    j.IsActive &&
                    (!j.ClosingDate.HasValue ||
                     j.ClosingDate.Value >=
                        DateTime.UtcNow));

            ViewBag.TotalApplications =
                applications.Count;

            ViewBag.NewApplicationsThisWeek =
                applications.Count(a =>
                    a.AppliedAt >= weekStart);

            ViewBag.AcceptedApplications =
                applications.Count(a =>
                    a.Status ==
                    ApplicationStatus.Accepted);

            ViewBag.AppliedCount =
                applications.Count(a =>
                    a.Status ==
                    ApplicationStatus.Applied);

            ViewBag.UnderReviewCount =
                applications.Count(a =>
                    a.Status ==
                    ApplicationStatus.UnderReview);

            ViewBag.InterviewCount =
                applications.Count(a =>
                    a.Status ==
                    ApplicationStatus.Interview);

            ViewBag.RejectedCount =
                applications.Count(a =>
                    a.Status ==
                    ApplicationStatus.Rejected);

            return View(
                applications
                    .OrderByDescending(
                        a => a.AppliedAt)
                    .ToList());
        }

        // ============================================================
        // UPDATE DASHBOARD STATUS
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateDashboardStatus(
            int id,
            ApplicationStatus status)
        {
            var application =
                await _context.JobApplications
                    .Include(a => a.JobPost)
                    .FirstOrDefaultAsync(
                        a => a.Id == id);

            if (application == null)
                return NotFound();

            var companyId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (application.JobPost.CompanyId != companyId)
                return Forbid();

            application.Status = status;

            await _context.SaveChangesAsync();

            var history =
                new ApplicationStatusHistory
                {
                    JobApplicationId =
                        application.Id,

                    Status = status,

                    ChangedAt =
                        DateTime.UtcNow
                };

            _context.ApplicationStatusHistories.Add(
                history);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Status updated successfully!";

            return RedirectToAction(
                nameof(CompanyDashboard));
        }

        // ============================================================
        // SYNC LINKEDIN JOBS FROM APIFY
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> SyncJobs()
        {
            using var client = new HttpClient();

            client.Timeout = TimeSpan.FromMinutes(5);

            string baseUrl = _configuration["ApifySettings:DatasetUrl"];
            string apiToken = _configuration["ApifySettings:ApiToken"];
            string apifyUrl = $"{baseUrl}?token={apiToken}";
            try
            {
                client.DefaultRequestHeaders
                    .UserAgent
                    .ParseAdd(
                        "Mozilla/5.0");

                var response =
                    await client.GetAsync(
                        apifyUrl);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "Failed to fetch jobs from Apify. Status code: "
                        + response.StatusCode;

                    return RedirectToAction(
                        nameof(Index));
                }

                var jsonString =
                    await response.Content
                        .ReadAsStringAsync();

                using var doc =
                    JsonDocument.Parse(
                        jsonString);

                var root = doc.RootElement;

                if (root.ValueKind !=
                    JsonValueKind.Array)
                {
                    TempData["Error"] =
                        "Apify did not return a valid jobs array.";

                    return RedirectToAction(
                        nameof(Index));
                }

                if (root.GetArrayLength() == 0)
                {
                    TempData["Error"] =
                        "Apify returned 0 jobs.";

                    return RedirectToAction(
                        nameof(Index));
                }

                var categories =
                    await _context.Categories
                        .OrderBy(c => c.Id)
                        .ToListAsync();

                if (!categories.Any())
                {
                    TempData["Error"] =
                        "Please add at least one category first.";

                    return RedirectToAction(
                        nameof(Index));
                }

                var cities =
                    await _context.Cities
                        .OrderBy(c => c.Id)
                        .ToListAsync();

                if (!cities.Any())
                {
                    TempData["Error"] =
                        "Please add at least one city first.";

                    return RedirectToAction(
                        nameof(Index));
                }

                int addedCount = 0;
                int skippedCount = 0;

                int cityIndex = 0;
                int categoryIndex = 0;

                foreach (var item in
                    root.EnumerateArray())
                {
                    // =================================================
                    // TITLE
                    // =================================================

                    string? title =
                        GetJsonString(
                            item,
                            "title",
                            "job_title",
                            "jobTitle",
                            "name");

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        skippedCount++;
                        continue;
                    }

                    title = title.Trim();

                    // =================================================
                    // COMPANY
                    // =================================================

                    string companyName =
                        GetJsonString(
                            item,
                            "companyName",
                            "company",
                            "company_name",
                            "organization",
                            "company_name_text")
                        ?? "LinkedIn Company";

                    companyName =
                        companyName.Trim();

                    if (string.IsNullOrWhiteSpace(
                        companyName))
                    {
                        companyName =
                            "LinkedIn Company";
                    }

                    // =================================================
                    // DESCRIPTION
                    // =================================================

                    string description =
                        GetJsonString(
                            item,
                            "description",
                            "jobDescription",
                            "job_description",
                            "descriptionText")
                        ?? "No description available";

                    if (string.IsNullOrWhiteSpace(
                        description))
                    {
                        description =
                            "No description available";
                    }

                    // =================================================
                    // CHECK DUPLICATE
                    // =================================================

                    var duplicateExists =
                        await _context.JobPosts
                            .AnyAsync(j =>
                                j.Title == title &&
                                j.Company.CompanyName ==
                                    companyName);

                    if (duplicateExists)
                    {
                        skippedCount++;
                        continue;
                    }

                    // =================================================
                    // CATEGORY + CITY
                    // =================================================

                    var selectedCategory =
                        categories[
                            categoryIndex %
                            categories.Count];

                    var selectedCity =
                        cities[
                            cityIndex %
                            cities.Count];

                    categoryIndex++;
                    cityIndex++;

                    // =================================================
                    // COMPANY
                    // =================================================

                    var company =
                        await _context.CompanyProfiles
                            .FirstOrDefaultAsync(
                                c =>
                                    c.CompanyName ==
                                    companyName);

                    if (company == null)
                    {
                        var userId =
                            Guid.NewGuid()
                                .ToString();

                        var uniqueSuffix =
                            Guid.NewGuid()
                                .ToString()
                                .Substring(
                                    0,
                                    8);

                        var newIdentityUser =
                            new ApplicationUser
                            {
                                Id = userId,

                                UserName =
                                    "sync_" +
                                    uniqueSuffix,

                                NormalizedUserName =
                                    (
                                        "sync_" +
                                        uniqueSuffix
                                    ).ToUpper(),

                                Email =
                                    "sync_" +
                                    uniqueSuffix +
                                    "@jobentry.com",

                                NormalizedEmail =
                                    (
                                        "sync_" +
                                        uniqueSuffix +
                                        "@jobentry.com"
                                    ).ToUpper()
                            };

                        _context.Users.Add(
                            newIdentityUser);

                        company =
                            new CompanyProfile
                            {
                                Id = userId,

                                CompanyName =
                                    companyName,

                                IsApproved = true
                            };

                        _context.CompanyProfiles.Add(
                            company);

                        await _context.SaveChangesAsync();
                    }

                    // =================================================
                    // ADD JOB
                    // =================================================

                    var newJob =
                        new JobPost
                        {
                            Title = title,

                            Description =
                                description,

                            MinSalary = 10000,

                            MaxSalary = 25000,

                            CreatedAt =
                                DateTime.UtcNow,

                            IsActive = true,

                            CompanyId =
                                company.Id,

                            CityId =
                                selectedCity.Id,

                            CategoryId =
                                selectedCategory.Id,

                            JobType =
                                JobType.FullTime,

                            ExperienceLevel =
                                ExperienceLevel.MidLevel
                        };

                    _context.JobPosts.Add(
                        newJob);

                    addedCount++;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Sync completed successfully! " +
                    $"Added: {addedCount}, " +
                    $"Skipped: {skippedCount}.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (JsonException)
            {
                TempData["Error"] =
                    "Apify returned invalid JSON data.";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] =
                    "Could not connect to Apify: "
                    + ex.Message;

                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                var errorDetails =
                    ex.InnerException?.Message
                    ?? ex.Message;

                TempData["Error"] =
                    "Sync error: "
                    + errorDetails;

                return RedirectToAction(
                    nameof(Index));
            }
        }

        // ============================================================
        // HELPER: READ JSON STRING
        // ============================================================

        private static string? GetJsonString(
            JsonElement item,
            params string[] propertyNames)
        {
            foreach (var propertyName
                in propertyNames)
            {
                if (!item.TryGetProperty(
                    propertyName,
                    out var property))
                {
                    continue;
                }

                if (property.ValueKind ==
                    JsonValueKind.String)
                {
                    var value =
                        property.GetString();

                    if (!string.IsNullOrWhiteSpace(
                        value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }
    }
}