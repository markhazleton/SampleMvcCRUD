using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Core.Data.Models;

namespace Mwh.Sample.Core.Data.Tests.Models
{
    [TestClass]
    public class EmployeeContextTests
    {
        [TestMethod]
        public void EmployeeContext_ExpectedBehavior()
        {
            // Arrange
            var employeeContext = new EmployeeContext();

            // Act


            // Assert
            Assert.IsNotNull(employeeContext);
        }
    }
}
