using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Core.Data.Models;

namespace Mwh.Sample.Core.Data.Tests.Models
{
    [TestClass]
    public class EmployeeTests
    {
        [TestMethod]
        public void Employee_Test()
        {
            // Arrange
            var employee = new Employee
            {
                Age = 20,
                Country = "USA",
                DepartmentId = 1,
                Id = 1,
                Name = "Test Employee",
                State = "TX"
            };
            // Act
            employee.Age = 21;

            // Assert
            Assert.IsNotNull(employee);
        }
    }
}