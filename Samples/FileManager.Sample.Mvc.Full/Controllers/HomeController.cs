using System.Diagnostics;
using System.Security.Claims;
using FileManager.Sample.Mvc.Full.Models;
using FileManager.Sample.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileManager.Sample.Mvc.Full.Controllers;

public class HomeController : Controller
{
    private readonly SampleStorageOptions _storage;
    private readonly SampleShowcaseOptions _showcase;

    public HomeController(IOptions<SampleStorageOptions> storage, IOptions<SampleShowcaseOptions> showcase)
    {
        _storage = storage.Value;
        _showcase = showcase.Value;
    }

    public IActionResult Index()
    {
        ViewBag.StorageProvider = _storage.Provider ?? "Physical";
        ViewBag.IsReadOnly = SampleStorageConfigurator.IsReadOnlyProvider(_storage.Provider);
        ViewBag.ShowcaseMountMode = _showcase.MountMode ?? "Lite";
        ViewBag.RequireAuth = _showcase.RequireAuth;
        return View();
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        if (!_showcase.RequireAuth)
            return RedirectToAction(nameof(Index));
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string returnUrl = "/")
    {
        if (!_showcase.RequireAuth)
            return RedirectToAction(nameof(Index));

        if (string.Equals(username, _showcase.DemoUsername, StringComparison.Ordinal)
            && string.Equals(password, _showcase.DemoPassword, StringComparison.Ordinal))
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "SampleDemoCookie");
            await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
            return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }

        ViewBag.Error = "Invalid credentials.";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
