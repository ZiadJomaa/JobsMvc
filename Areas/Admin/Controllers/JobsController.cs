using JobsMvc.Data;
using JobsMvc.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JobsMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var jobs = await _context.JobPosts
                .Include(j => j.Category)
                .Include(j => j.City)
                .ToListAsync();

            return View(jobs);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var job = await _context.JobPosts
                .Include(j => j.Category)
                .Include(j => j.City)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (job == null) return NotFound();

            return View(job);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var job = await _context.JobPosts.FindAsync(id);
            if (job != null)
            {
                _context.JobPosts.Remove(job);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var job = await _context.JobPosts.FindAsync(id);
            if (job != null)
            {
                job.IsApproved = true; 
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var job = await _context.JobPosts.FindAsync(id);
            if (job != null)
            {
                job.IsApproved = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}