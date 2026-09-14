using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;
using JobsMvc.ViewModels;
using JobsMvc.Data;

namespace JobsMvc.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            UserType = model.UserType,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (model.UserType == UserType.JobSeeker)
                {
                    await _userManager.AddToRoleAsync(user, "JobSeeker");
                    _context.JobSeekerProfiles.Add(new JobSeekerProfile
                    {
                        Id = user.Id,
                        FullName = model.FullName
                    });
                }
                else if (model.UserType == UserType.Company)
                {
                    await _userManager.AddToRoleAsync(user, "Company");
                    _context.CompanyProfiles.Add(new CompanyProfile
                    {
                        Id = user.Id,
                        CompanyName = model.FullName,
                        IsApproved = false // Pending admin approval
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);

                if (model.UserType == UserType.JobSeeker)
                    return RedirectToAction("EditProfile", "JobSeeker");

                return RedirectToAction("CompanyDashboard", "JobPosts");
            }
            catch
            {
                await transaction.RollbackAsync();
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while setting up your profile. Please try again.");
                return View(model);
            }
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // التحقق المباشر من دور الشركة وتوجيهها للداش بورد فوراً متخطية أي إرجاع قديم
            if (await _userManager.IsInRoleAsync(user, "Company"))
            {
                var company = await _context.CompanyProfiles.FindAsync(user.Id);
                if (company != null && !company.IsApproved)
                {
                    TempData["WarningMessage"] = "Your company account is currently pending administrator approval.";
                }
                return RedirectToAction("CompanyDashboard", "JobPosts");
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin",
                    new { area = "Admin" });
            }

            if (user.UserType == UserType.JobSeeker)
                return RedirectToAction("EditProfile", "JobSeeker");

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}