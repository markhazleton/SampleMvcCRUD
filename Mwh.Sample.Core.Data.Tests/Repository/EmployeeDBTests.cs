using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Core.Data.Repository;

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
        public void Delete_StateUnderTest_ExpectedBehaviorNewEmployee()
        {
            // Arrange
            var newEmp = new EmployeeModel()
            {
                Age = 33,
                Name = "Test User",
                State = "Texas",
                Country = "USA",
                Department = EmployeeDepartment.IT
            };

            // Act

            // Get Current count of employees
            var initResult = employeeDB.EmployeeCollection().ToArray();

            // Add New Employee with Update
            var addResult = employeeDB.Update(newEmp);
            // Get updated count of employees
            var updatedResult = employeeDB.EmployeeCollection().ToArray();
            /// Delete the Employee
            var result = employeeDB.Delete(addResult.id);
            // Get result after delete
            var finalResult = employeeDB.EmployeeCollection().ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(finalResult.Length, initResult.Length);

        }


        [TestMethod]
        public void Delete_StateUnderTest_ExpectedBehaviorNotFound()
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
        public void Update_StateUnderTest_ExpectedBehaviorNewEmployee()
        {
            // Arrange
            var newEmp = new EmployeeModel()
            {
                Age = 33,
                Name = "Test User",
                State = "Texas",
                Country = "USA",
                Department = EmployeeDepartment.IT
            };

            // Act

            // Get Current count of employees
            var initResult = employeeDB.EmployeeCollection().ToArray();

            // Add New Employee with Update
            var addResult = employeeDB.Update(newEmp);

            // Get updated count of employees
            var updatedResult = employeeDB.EmployeeCollection().ToArray();
            /// Update the Employee
            addResult.Name = "Test User 2";
            addResult.Age = 44;
            addResult.State = "FL";
            addResult.Department = EmployeeDepartment.Accounting;
            var result = employeeDB.Update(addResult);
            // Get result after update
            var finalResult = employeeDB.Employee(result.id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(addResult.id, result.id);
            Assert.AreNotEqual(initResult.Length, updatedResult.Length);
            Assert.AreEqual(finalResult.Age, 44);
            Assert.AreEqual(finalResult.State, "FL");

        }

        [TestMethod]
        public void Update_StateUnderTest_ExpectedBehaviorNull()
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