namespace UISampleSpark.Core.Tests.Models
{
    [TestClass]
    public class DepartmentResponseTests
    {
        [TestMethod]
        public void DepartmentResponse_Initialize()
        {
            // Arrange
            DepartmentResponse departmentResponse = new DepartmentResponse();
            // Act

            // Assert
            Assert.AreEqual(departmentResponse.Success, false);
            Assert.AreEqual(departmentResponse.Message, "Empty Initialize");
            Assert.IsNull(departmentResponse.Resource);
        }
        [TestMethod]
        public void DepartmentResponse_GoodResponse()
        {
            // Arrange
            DepartmentDto dept = new DepartmentDto(EmployeeDepartmentEnum.IT);
            DepartmentResponse departmentResponse = new DepartmentResponse(dept);
            // Act

            // Assert
            Assert.AreEqual(departmentResponse.Success, true);
            Assert.AreEqual(departmentResponse.Message, string.Empty);
            Assert.IsNotNull(departmentResponse.Resource);
        }
        [TestMethod]
        public void DepartmentResponse_BadResponse()
        {
            // Arrange
            DepartmentResponse departmentResponse = new DepartmentResponse("Test");
            // Act

            // Assert
            Assert.AreEqual(departmentResponse.Success, false);
            Assert.AreEqual(departmentResponse.Message, "Test");
            Assert.IsNull(departmentResponse.Resource);
        }
    }
}
