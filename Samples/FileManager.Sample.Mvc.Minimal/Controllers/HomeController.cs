using System.Diagnostics;
using FileManager.Sample.Mvc.Minimal.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Sample.Mvc.Minimal.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
