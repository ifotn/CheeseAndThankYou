using CheeseAndThankYou.Data;
using CheeseAndThankYou.Models;
using Microsoft.AspNetCore.Authorization;
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

            // calc total # of items in cart => store in session var for navbar badge
            var itemCount = (from c in cartItems
                             select c.Quantity).Sum();
            HttpContext.Session.SetInt32("ItemCount", itemCount);

            // return view
            return View(cartItems);
        }

        // GET: //Shop/RemoveFromCart/5
        public IActionResult RemoveFromCart(int id)
        {
            // find selected cart item
            var cartItem = _context.CartItems.Find(id);

            if (cartItem == null)
            {
                return RedirectToAction("Error");
            }

            // remove it
            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();

            // refresh cart
            return RedirectToAction("Cart");
        }

        // POST: //Shop/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int Quantity, int CartItemId)
        {
            // find selected cart item
            var cartItem = _context.CartItems.Find(CartItemId);

            if (cartItem == null)
            {
                return RedirectToAction("Error");
            }

            // update quantity
            cartItem.Quantity = Quantity;
            _context.CartItems.Update(cartItem);
            _context.SaveChanges();

            // refresh cart
            return RedirectToAction("Cart");
        }

        // GET: //Shop/Checkout
        [Authorize]
        public IActionResult Checkout()
        {
            return View();
        }

        // POST: //Shop/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Checkout([Bind("FirstName,LastName,Address,City,Province,PostalCode,Phone")] Order order)
        {
            // auto-fill other 3 order properties
            order.OrderDate = DateTime.Now;
            order.CustomerId = User.Identity.Name;

            var cartItems = _context.CartItems.Where(c => c.CustomerId == GetCustomerId());
            order.OrderTotal = (from c in cartItems
                                select c.Quantity * c.Price).Sum();

            // store the order in a session var
            HttpContext.Session.SetObject("Order", order);

            return RedirectToAction("Payment");
        }
    }
}
