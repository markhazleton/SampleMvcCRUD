namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class BaseResponseTests
{
    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestResponse : BaseResponse<TestDto>
    {
        public TestResponse() : base() { }
        public TestResponse(TestDto? resource) : base(resource) { }
        public TestResponse(bool isSuccess) : base(isSuccess) { }
        public TestResponse(string message) : base(message) { }
    }

    [TestMethod]
    public void BaseResponse_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        TestResponse response = new TestResponse();

        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual("Empty Initialize", response.Message);
        Assert.IsNull(response.Resource);
    }

    [TestMethod]
    public void BaseResponse_ConstructorWithResource_NonNullResource_ShouldSetSuccess()
    {
        // Arrange
        TestDto resource = new TestDto { Id = 1, Name = "Test" };

        // Act
        TestResponse response = new TestResponse(resource);

        // Assert
        Assert.IsTrue(response.Success);
        Assert.AreEqual(string.Empty, response.Message);
        Assert.AreEqual(resource, response.Resource);
    }

    [TestMethod]
    public void BaseResponse_ConstructorWithNullResource_ShouldSetFailure()
    {
        // Arrange
        TestDto? resource = null;

        // Act
        TestResponse response = new TestResponse(resource);

        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual("Resource is null", response.Message);
        Assert.IsNull(response.Resource);
    }

    [TestMethod]
    public void BaseResponse_ConstructorWithSuccessFlag_True_ShouldSetSuccess()
    {
        // Arrange & Act
        TestResponse response = new TestResponse(true);

        // Assert
        Assert.IsTrue(response.Success);
        Assert.AreEqual(string.Empty, response.Message);
        Assert.IsNull(response.Resource);
    }

    [TestMethod]
    public void BaseResponse_ConstructorWithSuccessFlag_False_ShouldSetFailure()
    {
        // Arrange & Act
        TestResponse response = new TestResponse(false);

        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual(string.Empty, response.Message);
        Assert.IsNull(response.Resource);
    }

    [TestMethod]
    public void BaseResponse_ConstructorWithMessage_ShouldSetFailureAndMessage()
    {
        // Arrange
        string message = "Test error message";

        // Act
        TestResponse response = new TestResponse(message);

        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual(message, response.Message);
        Assert.IsNull(response.Resource);
    }

    [TestMethod]
    public void EmployeeResponse_ConstructorWithEmployee_ShouldSetSuccess()
    {
        // Arrange
        EmployeeDto employee = new EmployeeDto(1, "Test", 25, "TX", "USA", EmployeeDepartmentEnum.IT);

        // Act
        EmployeeResponse response = new EmployeeResponse(employee);

        // Assert
        Assert.IsTrue(response.Success);
        Assert.AreEqual(employee, response.Resource);
    }

    [TestMethod]
    public void EmployeeResponse_ConstructorWithMessage_ShouldSetFailure()
    {
        // Arrange
        string message = "Employee not found";

        // Act
        EmployeeResponse response = new EmployeeResponse(message);

        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual(message, response.Message);
        Assert.IsNull(response.Resource);
    }

    [TestMethod]
    public void EmployeeResponse_ConstructorWithSuccessFlag_ShouldSetSuccess()
    {
        // Arrange & Act
        EmployeeResponse response = new EmployeeResponse(true);

        // Assert
        Assert.IsTrue(response.Success);
        Assert.AreEqual(string.Empty, response.Message);
    }

    [TestMethod]
    public void DepartmentResponse_ConstructorWithDepartment_ShouldSetSuccess()
    {
        // Arrange
        DepartmentDto dept = new DepartmentDto(EmployeeDepartmentEnum.IT);

        // Act
        DepartmentResponse response = new DepartmentResponse(dept);

        // Assert
        Assert.IsTrue(response.Success);
        Assert.AreEqual(dept, response.Resource);
    }

    [TestMethod]
    public void DepartmentResponse_ConstructorWithMessage_ShouldSetFailure()
    {
        // Arrange
        string message = "Department not found";

        // Act
        DepartmentResponse response = new DepartmentResponse(message);

        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual(message, response.Message);
        Assert.IsNull(response.Resource);
    }

    [TestMethod]
    public void DepartmentResponse_DefaultConstructor_ShouldInitialize()
    {
        // Arrange & Act
        DepartmentResponse response = new DepartmentResponse();

        // Assert
        Assert.IsFalse(response.Success);
        Assert.AreEqual("Empty Initialize", response.Message);
    }
}
