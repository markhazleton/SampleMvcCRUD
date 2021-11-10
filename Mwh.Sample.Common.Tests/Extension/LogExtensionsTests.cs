
namespace Mwh.Sample.Common.Tests.Extension;

[TestClass]
public class LogExtensionsTests
{
    [TestMethod]
    public void GetSerializeObjectString_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        EmployeeModel objectToSerialize = default;

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
        EmployeeModel objectToSerialize = null;

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
        List<EmployeeModel> lstObjectToSerialize = new();

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
        EmployeeModel type = new();

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
