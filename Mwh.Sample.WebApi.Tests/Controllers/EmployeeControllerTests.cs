using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Mwh.Sample.WebApi.Controllers
{
    [TestClass]
    public class EmployeeControllerTests : IDisposable
    {
        private EmployeeController controller;


        [TestMethod]
        public void GetEmployeeDeleteStateUnderTestExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = controller.GetEmployeeDelete(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetEmployeeEditStateUnderTestExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = controller.GetEmployeeEdit(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetEmployeeListStateUnderTestExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.GetEmployeeList();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void IndexStateUnderTestExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestInitialize]
        public void TestInitialize() { controller = new EmployeeController(); }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    controller.Dispose();
                }
                disposedValue = true;
            }
        }
        ~EmployeeControllerTests()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
