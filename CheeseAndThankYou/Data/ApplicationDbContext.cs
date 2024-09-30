using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CheeseAndThankYou.Models;

namespace CheeseAndThankYou.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // DbSets based on Model classes.  DbSet names plural, Model names singular
        public DbSet<Product> Products { get; set; }
       public DbSet<Category> Categories { get; set; }
       public DbSet<CartItem> CartItems { get; set; }
       public DbSet<OrderDetail> OrderDetails { get; set; }
       public DbSet<Order> Orders { get; set; }
    }
}
