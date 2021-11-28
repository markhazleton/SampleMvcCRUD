
namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class ApplicationStatusTests
{
    [TestMethod]
    public void Test_ApplicationStatus_ExpectedBehavior()
    {
        // Arrange
        var applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());

        var mytest = applicationStatus.BuildVersion.ToString();

        // Act

        // Assert
        Assert.IsNotNull(applicationStatus);
        Assert.IsNotNull(mytest);
    }
}

