using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.WebApi.Controllers;
using Mwh.SampleCRUD.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.Sample.WebApi.Tests.Controllers
{
    [TestClass]
    public class EmployeeApiControllerTests
    {
        private EmployeeApiController controller;

        [TestInitialize]
        public void TestInitialize()
        {
            controller = new EmployeeApiController();
            controller.Request = new System.Net.Http.HttpRequestMessage();
            controller.Configuration = new System.Web.Http.HttpConfiguration();
        }

        [TestMethod]
        public void Delete()
        {
            // Arrange
            int id = 2;

            // Act
            var result = controller.Delete(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Delete_StateUnderTest_InvalidId()
        {
            // Arrange
            int id = 0;

            // Act
            var result = controller.Delete(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void Get_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Get_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            int id = 3;

            // Act
            var result = controller.Get(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.EmployeeID);
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange

            // Act
            EmployeeModel result = controller.Get(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Name, "Bob");
            Assert.AreEqual(result.Age, 50);
            Assert.AreEqual(result.State, "Texas");
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
                Department = EmployeeDepartment.IT,
                JobList = new JobAssignmentList()
            };
            var result = controller.Post(newEmployee);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.EmployeeID, 4);
            Assert.AreEqual(result.Name, "Bill");
            Assert.AreEqual(result.Age, 25);
            Assert.AreEqual(result.State, "Texas");
        }

        [TestMethod]
        public void Post_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeModel employee = null;

            // Act
            var result = controller.Post(employee);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.EmployeeID);
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
                Department = EmployeeDepartment.IT,
                JobList = new JobAssignmentList()
            };

            // Act
            var result = controller.Post(newEmployee);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.EmployeeID, 5);
            Assert.AreEqual(result.Name, "Bill");
            Assert.AreEqual(result.Age, 25);
            Assert.AreEqual(result.State, "Texas");
        }
    }
}
