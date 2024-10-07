using CheeseAndThankYou.Data;
using CheeseAndThankYou.Models;
using Microsoft.AspNetCore.Mvc;

namespace CheeseAndThankYou.Controllers
{
    public class ShopController : Controller
    {
        // db connection for all methods in controller
        private readonly ApplicationDbContext _context;

        // constructor w/db connection dependency
        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // fetch list of categories & pass to view for display
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // GET: /Shop/ByCategory/5
        public IActionResult ByCategory(int id)
        {
            switch (id)
            {
                case 1:
                    ViewData["Category"] = "English";
                    break;
                case 2:
                    ViewData["Category"] = "Soft";
                    break;
                case 3:
                    ViewData["Category"] = "Hard";
                    break;
                case 4:
                    ViewData["Category"] = "Blue";
                    break;
                default:
                    return RedirectToAction("Index");
            }

            // use Product model to make in-memory product list
            var products = new List<Product>();

            for (int i = 1; i < 13; i++)
            {
                products.Add(new Product { ProductId = i, Name = ViewData["Category"] + " Cheese " + i });
            }

            return View(products);
        }
    }
}
