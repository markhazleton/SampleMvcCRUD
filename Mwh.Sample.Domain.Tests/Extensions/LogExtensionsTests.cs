
namespace Mwh.Sample.Domain.Tests.Extensions;

[TestClass]
public class LogExtensionsTests
{
    [TestMethod]
    public void GetSerializeObjectString_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        EmployeeDto objectToSerialize = default;

        // Act
        var result = LogExtensions.GetSerializeObjectString(
            objectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetSerializeObjectString_StateUnderTest_ExpectedBehaviorNull()
    {
        // Arrange
        EmployeeDto objectToSerialize = null;

        // Act
        var result = LogExtensions.GetSerializeObjectString(
            objectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetSerializeObjectString_StateUnderTest_ExpectedBehavior1()
    {
        // Arrange
        List<EmployeeDto> lstObjectToSerialize = new();

        // Act
        var result = LogExtensions.GetSerializeObjectString(
            lstObjectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void IsSimpleType_StateUnderTest_ExpectedBehaviorFalse()
    {
        // Arrange
        EmployeeDto type = new();

        // Act
        var result = LogExtensions.IsSimpleType(
            type.GetType());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSimpleType_StateUnderTest_ExpectedBehaviorTrue()
    {
        // Arrange
        string type = "test";

        // Act
        var result = LogExtensions.IsSimpleType(
            type.GetType());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
    }
}
