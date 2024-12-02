using CheeseAndThankYou.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CheeseAndThankYou.Controllers
{
    public class HomeController : Controller
    {
        // not needed private readonly ILogger<HomeController> _logger;

        public HomeController()
        {
            // not needed: _logger = logger;
        }

        public IActionResult Index()
        {
            return View("Index");
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
    }
}
