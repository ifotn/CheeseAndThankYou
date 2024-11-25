using CheeseAndThankYou.Data;
using CheeseAndThankYou.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace CheeseAndThankYou.Controllers
{
    public class ShopController : Controller
    {
        // db connection for all methods in controller
        private readonly ApplicationDbContext _context;

        // config dependency to read Stripe key from appsettings
        private readonly IConfiguration _iconfiguration;

        // constructor w/db connection dependency
        public ShopController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _iconfiguration = configuration;
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

        // GET: //Shop/Payment
        [Authorize]
        public IActionResult Payment()
        {
            // get the order from session var; we need the total to send to Stripe
            var order = HttpContext.Session.GetObject<Order>("Order");

            // read Stripe key from configuration
            StripeConfiguration.ApiKey = _iconfiguration.GetValue<string>("StripeSecretKey");

            // create Stripe checkout session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    // equivalent to var x = new object();  x.prop1 = val1; x.prop2 = val2;
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "cad",
                            UnitAmount = (long)(order.OrderTotal * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Cheese and Thank You Purchase"
                            }
                        }
                    }
                },
                Mode = "payment",
                // using Request.Host uses domain dynamically (localhost or live domain)
                SuccessUrl = "https://" + Request.Host + "/Shop/SaveOrder",
                CancelUrl = "https://" + Request.Host + "/Shop/Cart"
            };

            // execute the payment attempt w/Stripe
            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        // GET: //Shop/SaveOrder
        [Authorize]
        public IActionResult SaveOrder()
        {
            // get order from session var
            var order = HttpContext.Session.GetObject<Order>("Order");

            // save new order
            _context.Orders.Add(order);
            _context.SaveChanges();

            // move each cart item to new order detail record
            var cartItems = _context.CartItems.Where(c => c.CustomerId == GetCustomerId());

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId, // FK
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    Price = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
                _context.CartItems.Remove(item);
            }
            _context.SaveChanges();

            // clear session vars
            HttpContext.Session.Clear();

            // redirect to order confirmation using new OrderId PK
            return RedirectToAction("Details", "Orders", new { @id = order.OrderId });
        }
    }
}
