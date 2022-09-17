namespace Mwh.Sample.Domain.Tests.Models
{
    [TestClass]
    public class DepartmentDtoTests
    {
        [TestMethod]
        public void DepartmentDto_ExpectedResults()
        {
            // Arrange
            var departmentDto = new DepartmentDto(
                1,
                "Test",
                "Test")
            {
                Employees = new EmployeeDto[]
            {
                new EmployeeDto(1,
                    "Test",
                    22,
                    "TX",
                    "USA",
                    EmployeeDepartmentEnum.IT),
                new EmployeeDto(
                    2,
                    "Test Two",
                    33,
                    "TX",
                    "USA",
                    EmployeeDepartmentEnum.IT)
            },

                // Act
                Name = "Test Name",
                Description = "Test Description"
            };

            // Assert
            Assert.IsNotNull(departmentDto);
            Assert.AreEqual(1, departmentDto.Id);
            Assert.AreEqual("Test Name", departmentDto.Name);
            Assert.AreEqual("Test Description", departmentDto.Description);
            Assert.IsNotNull(departmentDto.Employees);
            Assert.AreEqual(2, departmentDto.Employees.Length);
        }
    }
}
