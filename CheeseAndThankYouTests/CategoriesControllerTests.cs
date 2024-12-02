using CheeseAndThankYou.Controllers;
using CheeseAndThankYou.Data;
using CheeseAndThankYou.Models;
using Microsoft.AspNetCore.Mvc;
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

        #region "Index"
        [TestMethod]
        public void IndexReturnsView()
        {
            // no arrange - done by TestInitialize() automatically

            // act.  Have to add .Result property as Index() is async
            var result = (ViewResult)controller.Index().Result;

            // assert
            Assert.AreEqual("Index", result.ViewName);
        }

        [TestMethod]
        public void IndexReturnsCategories()
        {
            // no arrange - done by TestInitialize() automatically

            // act.  Have to add .Result property as Index() is async
            var result = (ViewResult)controller.Index().Result;
            var dataModel = (List<Category>)result.Model;

            // assert.  
            CollectionAssert.AreEqual(_context.Categories.ToList(), dataModel);
        }
        #endregion

        #region "Details"
        [TestMethod]
        public void DetailsNoIdReturns404()
        {
            // act
            var result = (ViewResult)controller.Details(null).Result;

            // assert
            Assert.AreEqual("404", result.ViewName);
        }

        [TestMethod]
        public void DetailsInvalidIdReturns404()
        {
            // act
            var result = (ViewResult)controller.Details(-1).Result;

            // assert
            Assert.AreEqual("404", result.ViewName);
        }

        [TestMethod]
        public void DetailsValidIdReturnsView()
        {
            // act, passing one of the ids used in the mock db above
            var result = (ViewResult)controller.Details(91).Result;

            // assert
            Assert.AreEqual("Details", result.ViewName);
        }

        [TestMethod]
        public void DetailsValidIdReturnsCategory()
        {
            // arrange - set valid id from mock db
            int id = 91;

            // act, passing one of the ids used in the mock db above
            var result = (ViewResult)controller.Details(id).Result;
            var category = (Category)result.Model;

            // assert
            Assert.AreEqual(_context.Categories.Find(id), category);
        }
        #endregion
    }
}
