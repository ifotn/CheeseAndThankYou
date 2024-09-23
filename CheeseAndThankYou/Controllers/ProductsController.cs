using Microsoft.AspNetCore.Mvc;

namespace CheeseAndThankYou.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // GET: /products/create
        public IActionResult Create()
        {
            return View();
        }
    }
}
