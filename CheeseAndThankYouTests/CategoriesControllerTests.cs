using CheeseAndThankYou.Controllers;
using CheeseAndThankYou.Data;
using CheeseAndThankYou.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheeseAndThankYouTests
{
    [TestClass]
    public class CategoriesControllerTests
    {
        // shared mock db object used in all tests
        private ApplicationDbContext _context;
        CategoriesController controller;

        // startup method that creates db automatically before each test runs
        [TestInitialize]
        public void TestInitialize()
        {
            // create new in-memory db to pass as dependency to our controller
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // add some data to the mock db
            _context.Categories.Add(new Category { CategoryId = 56, Name = "Cat 56" });
            _context.Categories.Add(new Category { CategoryId = 32, Name = "Cat 32" });
            _context.Categories.Add(new Category { CategoryId = 91, Name = "Cat 91" });
            _context.SaveChanges();

            // instantiate instance of CategoriesController and pass mock db as dependency to constructor
            controller = new CategoriesController(_context);
        }

    }
}
