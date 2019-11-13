using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.SampleCRUD.BL.Models;
using System;

namespace Mwh.SampleCRUD.BL.Tests.Models
{
    [TestClass]
    public class JobAssignmentModelTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Arrange
            var jobAssignmentModel = new JobAssignmentModel()
            {
                Title = "Title",
                StartDate = DateTime.Now.Date.AddDays(-100),
                CompLevel = 50,
                EndDate = DateTime.Now.Date.AddDays(100),
                OutcomeCode = 1
            };

            // Act

            // Assert
            Assert.IsNotNull(jobAssignmentModel);
        }
    }
}
