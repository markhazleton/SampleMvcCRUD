using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Core.Data.Repository;
using System;

namespace Mwh.Sample.Core.Data.Tests.Repository
{
    [TestClass]
    public class EmployeeDBTests
    {
        private EmployeeDB employeeDB;

        [TestInitialize]
        public void Initialize()
        {
            var builder = new DbContextOptionsBuilder();
            _ = builder.UseInMemoryDatabase("AddMultipleEmployees");
            employeeDB = new EmployeeDB(new Data.Models.EmployeeContext());
        }



        [TestMethod]
        public void Delete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int ID = 0;

            // Act
            var result = employeeDB.Delete(ID);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Employee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = employeeDB.Employee(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EmployeeCollection_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = employeeDB.EmployeeCollection();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Update_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeModel emp = null;

            // Act
            var result = employeeDB.Update(emp);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
