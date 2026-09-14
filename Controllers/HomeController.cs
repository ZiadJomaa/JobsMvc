using JobsMvc.Data;
using JobsMvc.Models;
using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;
using JobsMvc.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace JobsMvc.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? searchString, int? categoryId, int? cityId)
    {
        ViewData["CurrentSearch"] = searchString;
        ViewData["CurrentCategory"] = categoryId;
        ViewData["CurrentCity"] = cityId;

        var jobsQuery = _context.JobPosts
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.Category)
            .Where(j => j.IsApproved && (!j.ClosingDate.HasValue || j.ClosingDate.Value >= System.DateTime.UtcNow))
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            jobsQuery = jobsQuery.Where(j => j.Title.Contains(searchString) || j.Description.Contains(searchString));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            jobsQuery = jobsQuery.Where(j => j.CategoryId == categoryId.Value);
        }

        if (cityId.HasValue && cityId.Value > 0)
        {
            jobsQuery = jobsQuery.Where(j => j.CityId == cityId.Value);
        }

        var viewModel = new HomeViewModel
        {
            Categories = await _context.Categories
                .Select(c => new HomeCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    JobCount = c.JobPosts.Count(j => j.IsApproved && (!j.ClosingDate.HasValue || j.ClosingDate.Value >= System.DateTime.UtcNow))
                })
                .ToListAsync(),

            NewestJobs = await jobsQuery
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync(),

            Cities = await _context.Cities.ToListAsync()
        };

        return View(viewModel);
    }

   
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var job = await _context.JobPosts
            .Include(j => j.Category)
            .Include(j => j.City)
            .Include(j => j.Company)
            .Include(j => j.RequiredSkills)
                .ThenInclude(rs => rs.Skill)
            .FirstOrDefaultAsync(m => m.Id == id && m.IsApproved);

        if (job == null) return NotFound();

        return View(job);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("Home/NotFound")]
    public new IActionResult NotFound()
    {
        Response.StatusCode = 404;
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(JobsMvc.ViewModel.CustomRegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.SelectedUserType == UserType.Company)
            {
                return RedirectToAction("Profile", "Company");
            }

            return RedirectToAction("Index");
        }
        return View(model);
    }
}