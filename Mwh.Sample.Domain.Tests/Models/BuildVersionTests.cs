
namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class BuildVersionTests
{
    [TestMethod]
    public void ToString_ExpectedBehavior()
    {
        // Arrange
        var buildVersion = new BuildVersion(Assembly.GetExecutingAssembly());

        // Act
        var result = buildVersion.ToString();

        // Assert
        Assert.IsNotNull(result);
    }
}
