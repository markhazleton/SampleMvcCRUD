using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.SampleCRUD.BL.Models;

namespace Mwh.SampleCRUD.BL.Tests.Models
{
    [TestClass]
    public class EmployeeModelTests
    {
        [TestMethod]
        public void EmployeeModel_Validate()
        {
            // Arrange
            var employeeModel = new EmployeeModel()
            {
                Age = 20,
                State = "State",
                Country = "Country",
                Department = EmployeeDepartment.Marketing,
                EmployeeID = 0,
                JobList = new JobAssignmentList(),
                Name = "Name"
            };

            // Act


            // Assert
            Assert.IsNotNull(employeeModel);
            Assert.AreEqual(employeeModel.Name, "Name");
            Assert.AreEqual(employeeModel.State, "State");
            Assert.AreEqual(employeeModel.Country, "Country");
            Assert.AreEqual(employeeModel.Department, EmployeeDepartment.Marketing);
            Assert.AreEqual(employeeModel.Age, 20);
        }
    }
}
