using CheeseAndThankYou.Data;
using CheeseAndThankYou.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            // make sure we have a valid Category Id
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var products = _context.Products.Where(p => p.CategoryId == id).ToList();

            // fetch list of products in selected category & pass to view
            return View(products);
        }

        // POST: /Shop/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int Quantity, int ProductId)
        {
            // look up product price
            var product = _context.Products.Find(ProductId);

            if (product == null)
            {
                return RedirectToAction("Error");
            }

            var price = product.Price;

            // create a unique cart identifier / fetch current cart identifier
            var customerId = GetCustomerId();

            // does this product already exist in this customer's cart?
            var cartItem = _context.CartItems.SingleOrDefault(c => c.CustomerId == customerId && c.ProductId == ProductId);

            if (cartItem != null)
            {
                // update quantity
                cartItem.Quantity += Quantity;
                _context.CartItems.Update(cartItem);
            }
            else
            {
                // create and save new cart item
                cartItem = new CartItem
                {
                    Quantity = Quantity,
                    ProductId = ProductId,
                    Price = price,
                    CustomerId = customerId
                };
                _context.CartItems.Add(cartItem);
            }
               
            _context.SaveChanges();

            // redirect to cart page
            return RedirectToAction("Cart");
        }

        private string GetCustomerId()
        {
            // check session var for a CustomerId
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                // use GUID to create new CustomerId session var 
                HttpContext.Session.SetString("CustomerId", Guid.NewGuid().ToString());
            }
           
            return HttpContext.Session.GetString("CustomerId");
        }

        // GET: //Shop/Cart
        public IActionResult Cart()
        {
            // get current user's cart items including parent ref to show product details
            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.CustomerId == GetCustomerId());

            // return view
            return View(cartItems);
        }
    }
}
