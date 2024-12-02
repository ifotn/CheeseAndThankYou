using CheeseAndThankYou.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace CheeseAndThankYouTests
{
    // suite of unit tests for methods in HomeController of CheeseAndThankYou web project
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void IndexReturnsView()
        {
            // 1. arrange - set up any vars/objects needed
            var controller = new HomeController();

            // 2. act - execute the method we want to evaluate.  must cast IActionResult to ViewResult
            var result = (ViewResult)controller.Index();

            // 3. assert - did we get expected result?
            Assert.AreEqual("Index", result.ViewName);
        }
    }
}