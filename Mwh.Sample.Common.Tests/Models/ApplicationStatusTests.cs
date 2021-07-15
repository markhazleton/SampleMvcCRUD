using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using System.Reflection;

namespace Mwh.Sample.Common.Tests.Models
{
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
}
