using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;

namespace Mwh.Sample.Common.Extension
{
    public enum TestEnum
    {
        [Display(Name = "Unknown", Description = "Unknown")]
        Unknown = 0,
        [Display(Name = "1st", Description = "The First One")]
        First = 1,
        Second = 2
    }


    [TestClass]
    public class EnumExtensionTests
    {

        [TestMethod]
        public void EnumGetDisplayNameExpectedBehavior()
        {
            // Arrange
            var myTest = TestEnum.First;

            // Act
            var result = myTest.GetDisplayName();

            // Assert
            Assert.AreEqual(result, "1st");
        }
        [TestMethod]
        public void EnumGetDisplayNameMissingDisplayName()
        {
            // Arrange
            var myTest = TestEnum.Second;

            // Act
            var result = myTest.GetDisplayName();

            // Assert
            Assert.AreEqual(result, "Second");
        }
        [TestMethod]
        public void EnumToDictionaryExpectedBehavior()
        {
            // Arrange
            var myTest = TestEnum.First;

            // Act
            var result = myTest.ToDictionary();

            // Assert
            Assert.AreEqual(result.Count, 3);
        }
    }
}
