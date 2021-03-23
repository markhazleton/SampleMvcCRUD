using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mwh.Sample.Common.Models
    {
    [TestClass]
    public class DeskTests
        {
        [TestMethod]
        public void Desk_Validate()
            {
            // Arrange
            var desk = new Desk();
            desk.Id = 1;
            // Act


            // Assert
            Assert.IsNotNull(desk);
            Assert.IsNotNull(desk.Id);
            Assert.AreEqual(desk.Id, 1);
            }
        }
    }
