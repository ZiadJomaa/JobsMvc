using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JobsMvc.Models;
using JobsMvc.Models.Enums;

namespace JobsMvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
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