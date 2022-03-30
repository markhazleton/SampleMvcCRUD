using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Domain.Models;

namespace Mwh.Sample.Domain.Tests.Models
{
    [TestClass]
    public class DepartmentResponseTests
    {
        [TestMethod]
        public void DepartmentResponse_Initialize()
        {
            // Arrange
            var departmentResponse = new DepartmentResponse();
            // Act

            // Assert
            Assert.AreEqual(departmentResponse.Success,false);
            Assert.AreEqual(departmentResponse.Message, "Empty Initialize");
            Assert.IsNull(departmentResponse.Resource);
        }
        [TestMethod]
        public void DepartmentResponse_GoodResponse()
        {
            // Arrange
            var dept = new DepartmentDto() 
            {
                Id = 1,
                Name ="Test",
                Description = "Test"
            };
            var departmentResponse = new DepartmentResponse(dept);
            // Act

            // Assert
            Assert.AreEqual(departmentResponse.Success, true);
            Assert.AreEqual(departmentResponse.Message, String.Empty);
            Assert.IsNotNull(departmentResponse.Resource);
        }
        [TestMethod]
        public void DepartmentResponse_BadResponse()
        {
            // Arrange
            var departmentResponse = new DepartmentResponse("Test");
            // Act

            // Assert
            Assert.AreEqual(departmentResponse.Success, false);
            Assert.AreEqual(departmentResponse.Message, "Test");
            Assert.IsNull(departmentResponse.Resource);
        }
    }
}
