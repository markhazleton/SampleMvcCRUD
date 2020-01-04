using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.WebApi;
using Mwh.Sample.WebApi.Controllers;

namespace Mwh.Sample.WebApi.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest : IDisposable
    {
        private HomeController controller;

        [TestInitialize]
        public void TestInitialize()
        {
            controller = new HomeController();
        }


        [TestMethod]
        public void Index()
        {
            // Arrange

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
        [TestMethod]
        public void EmpSinglePageStateUnderTestExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.EmpSinglePage();

            // Assert
            Assert.IsNotNull(result);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    controller.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~HomeControllerTest()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
