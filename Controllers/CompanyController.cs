using JobsMvc.Data;
using JobsMvc.Models.Entities;
using JobsMvc.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobsMvc.Controllers
{
    [Authorize(Roles = "Company")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CompanyController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Profile()
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var company = await _context.CompanyProfiles
                .Include(c => c.OperatingCities)
                .ThenInclude(cc => cc.City)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                return NotFound();

            return View(company);
        }

        public async Task<IActionResult> Edit()
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            var company = await _context.CompanyProfiles
                .Include(c => c.OperatingCities)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                return NotFound();

            var vm = new CompanyProfileFormViewModel
            {
                Id = company.Id,
                CompanyName = company.CompanyName,
                Industry = company.Industry,
                LogoPath = company.LogoPath,
                IsApproved = company.IsApproved,
                SelectedCityIds = company.OperatingCities
                    .Select(cc => cc.CityId)
                    .ToList()
            };

            await PopulateCitiesAsync(vm);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyProfileFormViewModel vm, IFormFile? logoFile)
        {
            var companyId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (companyId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                await PopulateCitiesAsync(vm);
                return View(vm);
            }

            var company = await _context.CompanyProfiles
                .Include(c => c.OperatingCities)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
                return NotFound();

            if (logoFile != null && logoFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "logos");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(logoFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(fileStream);
                }

                company.LogoPath = $"/uploads/logos/{uniqueFileName}";
            }

            company.CompanyName = vm.CompanyName;
            company.Industry = vm.Industry;

            company.OperatingCities.Clear();

            if (vm.SelectedCityIds != null)
            {
                foreach (var cityId in vm.SelectedCityIds)
                {
                    company.OperatingCities.Add(new CompanyProfile_City
                    {
                        CompanyProfileId = company.Id,
                        CityId = cityId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profile updated successfully.";

            return RedirectToAction(nameof(Edit));
        }

        private async Task PopulateCitiesAsync(CompanyProfileFormViewModel vm)
        {
            vm.AllCities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }
    }
}