using JobsMvc.Data;
using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;
using JobsMvc.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobsMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.UsersCount = await _context.Users.CountAsync();
            ViewBag.CompaniesCount = await _context.CompanyProfiles.CountAsync();
            ViewBag.JobsCount = await _context.JobPosts.CountAsync();
            ViewBag.ApplicationsCount = await _context.JobApplications.CountAsync();

            return View();
        }

        public async Task<IActionResult> Companies(bool showPendingOnly = true)
        {
            var query = _context.CompanyProfiles.AsQueryable();

            if (showPendingOnly)
                query = query.Where(c => !c.IsApproved);

            var companies = await query
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            ViewData["ShowPendingOnly"] = showPendingOnly;
            return View(companies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id, bool showPendingOnly = true)
        {
            var company = await _context.CompanyProfiles.FindAsync(id);
            if (company == null)
                return NotFound();

            company.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Companies), new { showPendingOnly });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, bool showPendingOnly = true)
        {
            var company = await _context.CompanyProfiles.FindAsync(id);
            if (company == null)
                return NotFound();

            company.IsApproved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Companies), new { showPendingOnly });
        }

        // GET: Admin/Admin/Admins
        public async Task<IActionResult> Admins()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            return View(admins.OrderBy(a => a.Email).ToList());
        }

        // GET: Admin/Admin/CreateAdmin
        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View(new CreateAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateAdminViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
                return View(model);
            }

            var admin = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                UserType = UserType.Admin,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(admin, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(admin, "Admin");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(admin);

                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            TempData["SuccessMessage"] = "Admin account created successfully.";
            return RedirectToAction(nameof(Admins));
        }

        // GET: Admin/Admin/MyProfile
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var model = new AdminProfileViewModel
            {
                FullName = user.FullName ?? user.UserName ?? "Administrator",
                Email = user.Email,
                CurrentImagePath = user.ProfileImagePath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MyProfile(AdminProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            model.Email = user.Email;
            model.CurrentImagePath = user.ProfileImagePath;

            if (model.ProfileImage != null)
            {
                var extension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                    ModelState.AddModelError(nameof(model.ProfileImage), "Use JPG, PNG, or WEBP image.");

                if (model.ProfileImage.Length > 2 * 1024 * 1024)
                    ModelState.AddModelError(nameof(model.ProfileImage), "Image size must be 2 MB or less.");
            }

            if (!ModelState.IsValid)
                return View(model);

            user.FullName = model.FullName.Trim();

            if (model.ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "admin-profiles");

                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
                var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImage.CopyToAsync(stream);
                }

                // Delete the old profile image, if it exists.
                if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
                {
                    var oldImagePath = Path.Combine(
                        _environment.WebRootPath,
                        user.ProfileImagePath.TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                user.ProfileImagePath = $"/uploads/admin-profiles/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(MyProfile));
        }
    }
}