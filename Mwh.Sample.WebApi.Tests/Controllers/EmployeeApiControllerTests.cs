using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using System;
using System.Net.Http;
using System.Web.Http;

namespace Mwh.Sample.WebApi.Controllers
    {
    [TestClass]
    public class EmployeeApiControllerTests : IDisposable
        {
        private EmployeeApiController controller;

        [TestMethod]
        public void Delete()
            {
            // Arrange
            int id = 2;

            // Act
            var result = controller.Delete(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(true, result);
            }

        [TestMethod]
        public void DeleteStateUnderTestInvalidId()
            {
            // Arrange
            int id = 0;

            // Act
            var result = controller.Delete(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result);
            }

        [TestMethod]
        public void GetById()
            {
            // Arrange

            // Act
            EmployeeModel result = controller.Get(1);

            // Assert
            Assert.IsNotNull(result);
            if (result != null)
                {
                Assert.AreEqual(result.Name, "Bob");
                Assert.AreEqual(result.Age, 50);
                Assert.AreEqual(result.State, "Texas");
                }
            }

        [TestMethod]
        public void GetStateUnderTestExpectedBehavior()
            {
            // Arrange

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsNotNull(result);
            }

        [TestMethod]
        public void GetStateUnderTestExpectedBehavior1()
            {
            // Arrange
            int id = 3;

            // Act
            var result = controller.Get(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result?.EmployeeID);
            }

        [TestMethod]
        public void Post()
            {
            // Arrange

            // Act
            var newEmployee = new EmployeeModel()
                {
                EmployeeID = 0,
                Age = 25,
                Name = "Bill",
                Country = "USA",
                State = "Texas",
                Department = EmployeeDepartment.IT
                };
            var result = controller.Post(newEmployee);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result?.Name, "Bill");
            Assert.AreEqual(result?.Age, 25);
            Assert.AreEqual(result?.State, "Texas");
            }

        [TestMethod]
        public void PostStateUnderTestExpectedBehavior()
            {
            // Arrange
            EmployeeModel employee = null;

            // Act
            var result = controller.Post(employee);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result?.EmployeeID);
            }

        [TestMethod]
        public void Put()
            {
            // Arrange
            var newEmployee = new EmployeeModel()
                {
                EmployeeID = 0,
                Age = 25,
                Name = "Bill",
                Country = "USA",
                State = "Texas",
                Department = EmployeeDepartment.IT
                };

            // Act
            var result = controller.Post(newEmployee);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result?.Name, "Bill");
            Assert.AreEqual(result?.Age, 25);
            Assert.AreEqual(result?.State, "Texas");
            }

        [TestInitialize]
        public void TestInitialize()
            {
            controller = new EmployeeApiController
                { Request = new HttpRequestMessage(), Configuration = new HttpConfiguration() };
            }

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

        ~EmployeeApiControllerTests() { Dispose(false); }

        public void Dispose()
            {
            Dispose(true);
            GC.SuppressFinalize(this);
            }
        #endregion
        }
    }
