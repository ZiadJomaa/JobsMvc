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

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Edit(CompanyProfileFormViewModel vm)
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

            company.CompanyName = vm.CompanyName;
            company.Industry = vm.Industry;
            company.LogoPath = vm.LogoPath;

            company.OperatingCities.Clear();

            foreach (var cityId in vm.SelectedCityIds)
            {
                company.OperatingCities.Add(new CompanyProfile_City
                {
                    CompanyProfileId = company.Id,
                    CityId = cityId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit));
        }

        private async Task PopulateCitiesAsync(CompanyProfileFormViewModel vm)
        {
            vm.AllCities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }
    }
}
