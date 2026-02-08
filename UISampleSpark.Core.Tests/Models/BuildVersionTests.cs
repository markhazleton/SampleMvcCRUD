
namespace UISampleSpark.Core.Tests.Models;

[TestClass]
public class BuildVersionTests
{
    [TestMethod]
    public void ToString_ExpectedBehavior()
    {
        // Arrange
        BuildVersion buildVersion = new BuildVersion(Assembly.GetExecutingAssembly());

        // Act
        string result = buildVersion.ToString();

        // Assert
        Assert.IsNotNull(result);
    }
}
