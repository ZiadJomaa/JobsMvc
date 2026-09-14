using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobsMvc.Data;
using System.Threading.Tasks;
using System.Linq;

namespace JobsMvc.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Companies
        public async Task<IActionResult> Index(string searchName, string industry)
        {
            var query = _context.CompanyProfiles.AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(c => c.CompanyName.Contains(searchName));
            }

            if (!string.IsNullOrEmpty(industry))
            {
                query = query.Where(c => c.Industry == industry);
            }

            ViewBag.Industries = await _context.CompanyProfiles
                .Where(c => !string.IsNullOrEmpty(c.Industry))
                .Select(c => c.Industry)
                .Distinct()
                .ToListAsync();

            ViewBag.SearchName = searchName;
            ViewBag.SelectedIndustry = industry;

            var companies = await query.ToListAsync();
            return View(companies);
        }

        // GET: Companies/Details/id
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var company = await _context.CompanyProfiles
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            var openJobs = await _context.JobPosts
                .Include(j => j.City)
                .Include(j => j.Category)
                .Where(j => j.CompanyId == id && (!j.ClosingDate.HasValue || j.ClosingDate.Value >= System.DateTime.UtcNow))
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            ViewBag.OpenJobs = openJobs;

            return View(company);
        }
    }
}