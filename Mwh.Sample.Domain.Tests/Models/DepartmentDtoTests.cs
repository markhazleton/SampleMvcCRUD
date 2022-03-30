namespace Mwh.Sample.Domain.Tests.Models
{
    [TestClass]
    public class DepartmentDtoTests
    {
        [TestMethod]
        public void DepartmentDto_ExpectedResults()
        {
            // Arrange
            var departmentDto = new DepartmentDto() 
            { 
                Id = 1,
                Name = "Test",
                Description ="Test",
            };
            departmentDto.Employees = new List<EmployeeDto>
            {
                new EmployeeDto()
                {
                    id = 1,
                    Name = "Test",
                    Age = 22,
                    State = "TX",
                    Country = "USA",
                    Department = EmployeeDepartmentEnum.IT
                },
                new EmployeeDto()
                {
                    id = 2,
                    Name = "Test Two",
                    Age = 33,
                    State = "TX",
                    Country = "USA",
                    Department = EmployeeDepartmentEnum.IT
                }
            };

            // Act
            departmentDto.Name = "Test Name";
            departmentDto.Description = "Test Description";

            // Assert
            Assert.IsNotNull(departmentDto);
            Assert.AreEqual(1, departmentDto.Id);
            Assert.AreEqual("Test Name", departmentDto.Name);
            Assert.AreEqual("Test Description",departmentDto.Description);
            Assert.IsNotNull(departmentDto.Employees);
            Assert.AreEqual(2, departmentDto.Employees.Count);
        }
    }
}
