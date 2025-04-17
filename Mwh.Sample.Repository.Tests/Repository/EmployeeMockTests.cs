using Mwh.Sample.Repository.Repository;
using System;
using System.Threading.Tasks;

namespace Mwh.Sample.Repository.Tests.Repository
{
    [TestClass]
    public class EmployeeMockTests
    {
        [TestMethod]
        public async Task DepartmentAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            int id = 1;

            // Act
            DepartmentDto? result = await employeeMock.DepartmentAsync(id);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task EmployeeAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            int id = 0;

            // Act
            EmployeeDto? result = await employeeMock.EmployeeAsync(id);

            // Assert
            Assert.IsNull(result);
        }
        [TestMethod]
        public async Task Delete_StateUnderTest_Id1()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            int ID = 1;

            // Act
            bool result = await employeeMock.DeleteEmployeeAsync(ID);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Delete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            int ID = 0;

            // Act
            bool result = await employeeMock.DeleteEmployeeAsync(ID);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DeleteAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            int ID = 0;

            // Act
            bool result = await employeeMock.DeleteEmployeeAsync(
                ID);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DepartmentCollection_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();

            // Act
            System.Collections.Generic.List<DepartmentDto> result = employeeMock.DepartmentCollection();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DepartmentCollectionAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();

            // Act
            System.Collections.Generic.List<DepartmentDto> result = await employeeMock.DepartmentCollectionAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EmployeeCollection_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();

            // Act
            System.Collections.Generic.List<EmployeeDto> result = employeeMock.EmployeeCollection();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task EmployeeCollectionAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();

            // Act
            System.Collections.Generic.List<EmployeeDto> result = await employeeMock.EmployeeCollectionAsync();

            // Assert
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_NotFoundID()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            EmployeeDto emp = new(
                9999,
                "Test",
                99,
                "Test",
                "Test",
                EmployeeDepartmentEnum.IT);

            // Act
            EmployeeDto? result = await employeeMock.UpdateAsync(emp);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, 9999);
            Assert.IsTrue(result.IsValid());
        }
        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_NotValid()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            EmployeeDto? emp = null;

            // Act
            EmployeeDto? result = await employeeMock.UpdateAsync(emp);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            EmployeeDto? emp = null;

            // Act
            EmployeeDto? result = await employeeMock.UpdateAsync(emp);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task UpdateAsync_StateUnderTest_ExpectedBehavior1()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            DepartmentDto? dept = null;

            // Act
            DepartmentDto? result = await employeeMock.UpdateAsync(dept);

            // Assert
            Assert.IsNull(result);
        }


        [TestMethod]
        public async Task Delete_StateUnderTest_ExpectedBehaviorNotFound()
        {
            // Arrange
            EmployeeMock employeeMock = new EmployeeMock();
            int ID = 0;

            // Act
            bool result = await employeeMock.DeleteEmployeeAsync(ID);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Employee_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeDB = new EmployeeMock();
            int id = 0;

            // Act
            EmployeeDto? result = await employeeDB.EmployeeAsync(id);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task EmployeeCollection_ExpectedBehavior()
        {
            // Arrange
            EmployeeMock employeeDB = new EmployeeMock();

            // Act
            System.Collections.Generic.List<EmployeeDto> result = await employeeDB.EmployeeCollectionAsync();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task Update_StateUnderTest_ExpectedBehaviorNewEmployee()
        {
            // Arrange
            EmployeeMock employeeDB = new EmployeeMock();
            EmployeeDto newEmp = new(
                9999,
                "Test",
                99,
                "Test",
                "Test",
                EmployeeDepartmentEnum.IT);


            // Act

            // Get Current count of employees
            System.Collections.Generic.List<EmployeeDto> initResult = await employeeDB.EmployeeCollectionAsync();

            // Add New Employee with Update
            EmployeeDto? addResult = await employeeDB.UpdateAsync(newEmp);

            // Get updated count of employees
            System.Collections.Generic.List<EmployeeDto> updatedResult = await employeeDB.EmployeeCollectionAsync();
            /// Update the Employee
            addResult.Name = "Test User 2";
            addResult.Age = 44;
            addResult.State = "FL";
            addResult.Department = EmployeeDepartmentEnum.Accounting;
            EmployeeDto? result = await employeeDB.UpdateAsync(addResult);
            // Get result after update
            EmployeeDto? finalResult = await employeeDB.EmployeeAsync(result.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(addResult.Id, result.Id);
            Assert.AreEqual(finalResult?.Age, 44);
            Assert.AreEqual(finalResult?.State, "FL");

        }
    }
}
