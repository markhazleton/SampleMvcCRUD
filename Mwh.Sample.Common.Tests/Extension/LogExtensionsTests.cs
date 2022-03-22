
namespace Mwh.Sample.Common.Tests.Extension;

/// <summary>
/// Test the Log Extension Methods
/// </summary>
[TestClass]
public class LogExtensionsTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void GetSerializeObjectString_Default()
    {
        // Arrange
        EmployeeModel? objectToSerialize = default;

        // Act
        var result = LogExtensions.GetSerializeObjectString(objectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void GetSerializeObjectString_Null()
    {
        // Arrange
        EmployeeModel? objectToSerialize = null;

        // Act
        var result = LogExtensions.GetSerializeObjectString(objectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void GetSerializeObjectString_NewList()
    {
        // Arrange
        List<EmployeeModel> lstObjectToSerialize = new();

        // Act
        var result = LogExtensions.GetSerializeObjectString(lstObjectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void IsSimpleType_NewModel_False()
    {
        // Arrange
        EmployeeModel type = new();

        // Act
        var result = LogExtensions.IsSimpleType(type.GetType());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }
    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void IsSimpleType_StringNull_True()
    {
        // Arrange
        string? type = null;

        // Act
        var result = type?.GetType().IsSimpleType();

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void IsSimpleType_StringNotNull_True()
    {
        // Arrange
        string type = "test";

        // Act
        var result = type.GetType().IsSimpleType();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
    }
}
