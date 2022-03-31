using System;

namespace Mwh.Sample.Repository.Tests.Models
{
    [TestClass]
    public class DepartmentTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var department = new Department()
            {
                Name = "Department",
                CreatedBy = "Test",
                Description = "Test",
                Id = 1,
                LastUpdatedBy = "Test",
            };

            // Act


            // Assert
            Assert.IsNotNull(department);
            Assert.AreEqual(department.CreatedDate.Date, DateTime.Now.Date);
        }
    }
}
