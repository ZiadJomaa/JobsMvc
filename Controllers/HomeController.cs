using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JobsMvc.Models;

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
}
