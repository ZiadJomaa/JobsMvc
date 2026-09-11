using JobsMvc.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobsMvc.Controllers
{
     [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
            if (company == null) return NotFound();

            company.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Companies), new { showPendingOnly });
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id, bool showPendingOnly = true)
        {
            var company = await _context.CompanyProfiles.FindAsync(id);
            if (company == null) return NotFound();

            company.IsApproved = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Companies), new { showPendingOnly });
        }
    }
}