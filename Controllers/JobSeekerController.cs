using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobsMvc.Data;
using JobsMvc.Models.Entities;
using JobsMvc.ViewModels;

namespace JobsMvc.Controllers;

[Authorize(Roles = "JobSeeker")]
public class JobSeekerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public JobSeekerController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var profile = await _context.JobSeekerProfiles
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.Id == CurrentUserId);

        if (profile == null)
            return NotFound();

        var viewModel = new JobSeekerProfileViewModel
        {
            Id = profile.Id,
            FullName = profile.FullName,
            Title = profile.Title,
            ExperienceYears = profile.ExperienceYears,
            CityId = profile.CityId,
            ExistingPhotoPath = profile.PhotoPath,
            SelectedSkillIds = profile.Skills.Select(s => s.SkillId).ToList(),
            CitiesList = await _context.Cities
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync(),
            SkillsList = await _context.Skills
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(JobSeekerProfileViewModel model)
    {
        var profile = await _context.JobSeekerProfiles
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.Id == CurrentUserId);

        if (profile == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.CitiesList = await _context.Cities.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToListAsync();
            model.SkillsList = await _context.Skills.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToListAsync();
            return View(model);
        }

        // Handle photo upload
        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.PhotoFile.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.PhotoFile.CopyToAsync(fileStream);
            }

            profile.PhotoPath = $"/uploads/profiles/{uniqueFileName}";
        }

        profile.FullName = model.FullName;
        profile.Title = model.Title;
        profile.ExperienceYears = model.ExperienceYears;
        profile.CityId = model.CityId;

        // Update skills
        profile.Skills.Clear();
        foreach (var skillId in model.SelectedSkillIds)
        {
            profile.Skills.Add(new JobSeeker_Skill { JobSeekerProfileId = profile.Id, SkillId = skillId });
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(EditProfile));
    }

    [HttpGet]
    public async Task<IActionResult> Resumes()
    {
        var resumes = await _context.Resumes
            .Where(r => r.JobSeekerId == CurrentUserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(resumes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadResume(string title, IFormFile resumeFile)
    {
        if (resumeFile == null || resumeFile.Length == 0 || Path.GetExtension(resumeFile.FileName).ToLower() != ".pdf")
        {
            TempData["ErrorMessage"] = "Please upload a valid PDF file.";
            return RedirectToAction(nameof(Resumes));
        }

        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
        Directory.CreateDirectory(uploadsFolder);
        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(resumeFile.FileName)}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await resumeFile.CopyToAsync(stream);
        }

        bool hasResumes = await _context.Resumes.AnyAsync(r => r.JobSeekerId == CurrentUserId);

        var resume = new Resume
        {
            Title = string.IsNullOrWhiteSpace(title) ? resumeFile.FileName : title,
            FilePath = $"/uploads/resumes/{uniqueFileName}",
            JobSeekerId = CurrentUserId,
            IsDefault = !hasResumes, // Set as default if it's the first resume
            CreatedAt = DateTime.UtcNow
        };

        _context.Resumes.Add(resume);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Resume uploaded successfully.";
        return RedirectToAction(nameof(Resumes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultResume(int id)
    {
        var resumes = await _context.Resumes.Where(r => r.JobSeekerId == CurrentUserId).ToListAsync();
        foreach (var res in resumes)
        {
            res.IsDefault = (res.Id == id);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Resumes));
    }
}