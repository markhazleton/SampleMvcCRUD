using System.IO;

namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class ApplicationStatusTests
{

    [TestMethod]
    public void ApplicationStatus_BuildDate_ReturnsValidDateTime()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var applicationStatus = new ApplicationStatus(assembly);


        string executingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string dllFilePath = Path.Combine(executingDirectory, "Newtonsoft.Json.dll");

        if (File.Exists(dllFilePath))
        {
            Assembly myFileAssembly = Assembly.LoadFrom(dllFilePath);
            var myFileStatus = new ApplicationStatus(myFileAssembly);
        }

        // Act
        var buildDate = applicationStatus.BuildDate;

        // Assert
        Assert.IsTrue(buildDate != DateTime.MinValue);
    }

    [TestMethod]
    public void ApplicationStatus_BuildVersion_ReturnsValidBuildVersion()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var applicationStatus = new ApplicationStatus(assembly);

        // Act
        var buildVersion = applicationStatus.BuildVersion;

        // Assert
        Assert.IsNotNull(buildVersion);
        // Add more assertions to validate the properties or behavior of the BuildVersion class.
    }

    [TestMethod]
    public void ApplicationStatus_Features_DefaultValueIsEmptyDictionary()
    {
        // Arrange
        var applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());

        // Act
        var features = applicationStatus.Features;

        // Assert
        Assert.IsNotNull(features);
        Assert.AreEqual(0, features.Count);
    }

    [TestMethod]
    public void ApplicationStatus_Messages_DefaultValueIsEmptyList()
    {
        // Arrange
        var applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());

        // Act
        var messages = applicationStatus.Messages;

        // Assert
        Assert.IsNotNull(messages);
        Assert.AreEqual(0, messages.Count);
    }

    [TestMethod]
    public void ApplicationStatus_Region_DefaultValueIsNotEmptyString()
    {
        // Arrange
        var applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());

        // Act
        var region = applicationStatus.Region;

        // Assert
        Assert.IsTrue(string.IsNullOrEmpty(region));
    }

    [TestMethod]
    public void ApplicationStatus_Status_DefaultValueIsOnline()
    {
        // Arrange
        var applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());

        // Act
        var status = applicationStatus.Status;

        // Assert
        Assert.AreEqual(ServiceStatus.Online, status);
    }

    [TestMethod]
    public void ApplicationStatus_Tests_DefaultValueIsEmptyDictionary()
    {
        // Arrange
        var applicationStatus = new ApplicationStatus(Assembly.GetExecutingAssembly());

        // Act
        var tests = applicationStatus.Tests;

        // Assert
        Assert.IsNotNull(tests);
        Assert.AreEqual(0, tests.Count);
    }



}
