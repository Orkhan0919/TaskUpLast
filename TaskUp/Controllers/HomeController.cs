using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskUp.Models;

namespace TaskUp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetTheme([FromBody] string theme)
    {
        Response.Cookies.Append("Theme", theme, new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddDays(365),
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            HttpOnly = false
        });
        return Json(new { success = true });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}