using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Mwh.Sample.Common.Models
    {
    [TestClass]
    public class DeskBookingTests
        {
        [TestMethod]
        public void DeskBooking_Validate()
            {
            // Arrange
            var deskBooking = new DeskBooking()
                {
                Id = 1,
                Date = DateTime.UtcNow,
                Email = "mark.hazleton@controlorigins.com",
                FirstName = "Mark",
                LastName = "Hazleton",
                DeskId = 1
                };
            // Act


            // Assert
            Assert.IsNotNull(deskBooking);
            Assert.IsNotNull(deskBooking.Id);
            Assert.AreEqual(deskBooking.Id, 1);
            Assert.AreEqual(deskBooking.Email, "mark.hazleton@controlorigins.com");
            Assert.AreEqual(deskBooking.FirstName, "Mark");
            Assert.AreEqual(deskBooking.LastName, "Hazleton");

            }
        }
    }
