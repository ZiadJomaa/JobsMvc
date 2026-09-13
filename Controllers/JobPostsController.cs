using JobsMvc.Data;
using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;
using JobsMvc.ViewModels.JobPost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobsMvc.Controllers
{
    public class JobPostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobPostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTitle, int? cityId, int? categoryId)
        {
            var query = _context.JobPosts
                .Include(j => j.Company)
                .Include(j => j.City)
                .Include(j => j.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(j => j.Title.Contains(searchTitle));
            }

            if (cityId.HasValue)
            {
                query = query.Where(j => j.CityId == cityId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(j => j.CategoryId == categoryId.Value);
            }

            var jobPosts = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            ViewBag.Cities = new SelectList(await _context.Cities.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", cityId);
            ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);

            return View(jobPosts);
        }

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

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create()
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                return NotFound();

            if (!company.IsApproved)
                return Forbid();

            var vm = new JobPostFormViewModel();

            await PopulateDropdownsAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Create(JobPostFormViewModel vm)
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.Id == companyId);

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
                jobPost.RequiredSkills.Add(new JobPost_Skill
                {
                    SkillId = skillId
                });
            }

            _context.JobPosts.Add(jobPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost = await _context.JobPosts
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
                SelectedSkillIds = jobPost.RequiredSkills
                    .Select(rs => rs.SkillId)
                    .ToList()
            };

            await PopulateDropdownsAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Edit(int id, JobPostFormViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost = await _context.JobPosts
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

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost = await _context.JobPosts
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost = await _context.JobPosts
                .FirstOrDefaultAsync(j =>
                    j.Id == id &&
                    j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            _context.JobPosts.Remove(jobPost);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> Close(int id)
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var jobPost = await _context.JobPosts
                .FirstOrDefaultAsync(j =>
                    j.Id == id &&
                    j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            jobPost.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Apply(int jobPostId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var jobSeekerProfile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(s => s.Id == userId);

            if (jobSeekerProfile == null)
            {
                jobSeekerProfile = new JobSeekerProfile
                {
                    Id = userId
                };
                _context.JobSeekerProfiles.Add(jobSeekerProfile);
                await _context.SaveChangesAsync();
            }

            var existingApplication = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.JobPostId == jobPostId && a.JobSeekerId == userId);

            if (existingApplication != null)
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction(nameof(Details), new { id = jobPostId });
            }

            var application = new JobApplication
            {
                JobPostId = jobPostId,
                JobSeekerId = userId,
                AppliedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Pending
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your application has been submitted successfully!";
            return RedirectToAction(nameof(Details), new { id = jobPostId });
        }

        private async Task PopulateDropdownsAsync(JobPostFormViewModel vm)
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

            vm.AllSkills = await _context.Skills
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> ViewApplications(int id)
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobPost = await _context.JobPosts
                .Include(j => j.Applications)
                    .ThenInclude(a => a.JobSeeker)
                .FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == companyId);

            if (jobPost == null)
                return NotFound();

            return View(jobPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, ApplicationStatus status)
        {
            var application = await _context.JobApplications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                return NotFound();

            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (application.JobPost.CompanyId != companyId)
                return Forbid();

            application.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application status updated successfully!";
            return RedirectToAction(nameof(ViewApplications), new { id = application.JobPostId });
        }

        [Authorize(Roles = "Company")]
        public async Task<IActionResult> CompanyDashboard()
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            // جلب التطبيقات مع ربط الـ JobSeeker مباشرة بطريقة آمنة
            var applications = await _context.JobApplications
                .Include(a => a.JobPost)
                .Include(a => a.JobSeeker)
                .Where(a => a.JobPost.CompanyId == companyId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Company")]
        public async Task<IActionResult> UpdateDashboardStatus(int id, ApplicationStatus status)
        {
            var application = await _context.JobApplications
                .Include(a => a.JobPost)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return NotFound();

            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (application.JobPost.CompanyId != companyId)
                return Forbid();

            application.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Status updated successfully!";
            return RedirectToAction(nameof(CompanyDashboard));
        }
    }
}