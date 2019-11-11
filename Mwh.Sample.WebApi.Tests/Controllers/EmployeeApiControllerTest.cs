using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.WebApi.Controllers;
using Mwh.SampleCRUD.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.Sample.WebApi.Tests.Controllers
{
    [TestClass]
    public class EmployeeApiControllerTest
    {

        [TestMethod]
        public void Delete()
        {
            // Arrange
            EmployeeApiController controller = new EmployeeApiController();

            // Act

            // Assert
        }
        [TestMethod]
        public void Get()
        {
            // Arrange
            EmployeeApiController controller = new EmployeeApiController();

            // Act
            IEnumerable<Employee> result = controller.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void GetById()
        {
            // Arrange
            EmployeeApiController controller = new EmployeeApiController();

            // Act
            Employee result = controller.Get(1);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Post()
        {
            // Arrange
            EmployeeApiController controller = new EmployeeApiController();

            // Act
            var newEmployee = new Employee();

            // Assert
        }

        [TestMethod]
        public void Put()
        {
            // Arrange
            EmployeeApiController controller = new EmployeeApiController();

            // Act

            // Assert
        }
    }
}
