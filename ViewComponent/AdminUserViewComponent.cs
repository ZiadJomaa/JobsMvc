using JobsMvc.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobsMvc.ViewComponents
{
    public class AdminUserViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUserViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            return View(user);
        }
    }
}