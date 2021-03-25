using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.WebApi.Controllers;

namespace Mwh.Sample.WebApi.Controllers
    {
    [TestClass]
    public class ErrorControllerTests
        {
        [TestMethod]
        public void Index_StateUnderTest_ExpectedBehavior()
            {
            // Arrange
            var errorController = new ErrorController();

            // Act
            var result = errorController.Index();

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void NotFound_StateUnderTest_ExpectedBehavior()
            {
            // Arrange
            var errorController = new ErrorController();

            // Act
            var result = errorController.NotFound();

            // Assert
            Assert.IsNotNull(result);
            }
        }
    }
