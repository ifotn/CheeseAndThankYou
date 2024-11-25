using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CheeseAndThankYou.Data;
using CheeseAndThankYou.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Immutable;

namespace CheeseAndThankYou.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrator"))
            {
                // all orders regardless of user
                var orders = await _context.Orders
                    .OrderByDescending(o => o.OrderId).ToListAsync();
                return View(orders);
            }
            else
            {
                // only the currently logged in customer's orders
                var orders = await _context.Orders
                    .Where(o => o.CustomerId == User.Identity.Name)
                    .OrderByDescending(o => o.OrderId).ToListAsync();
                return View(orders);
            }
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Administrator"))
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude( od => od.Product)
                    .FirstOrDefaultAsync(m => m.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            else
            {
                var order = await _context.Orders
                    .Where(o => o.CustomerId == User.Identity.Name)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(m => m.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
