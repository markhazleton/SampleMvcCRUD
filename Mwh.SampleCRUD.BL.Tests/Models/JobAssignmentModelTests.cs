using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.SampleCRUD.BL.Models;
using System;

namespace Mwh.SampleCRUD.BL.Tests.Models
{
    [TestClass]
    public class JobAssignmentModelTests
    {
        [TestMethod]
        public void JobAssignmentModel_Test()
        {
            // Arrange
            var jobAssignment = new JobAssignmentModel()
            {
                Title = "Title",
                StartDate = DateTime.Now.Date.AddDays(-100),
                CompLevel = 50,
                EndDate = DateTime.Now.Date.AddDays(100),
                OutcomeCode = 1
            };

            jobAssignment.Title = "Title Update";
            jobAssignment.StartDate = DateTime.Now.Date.AddDays(-200);
            jobAssignment.EndDate = DateTime.Now.Date;
            jobAssignment.CompLevel = 60;
            jobAssignment.OutcomeCode = 2;

            // Act

            // Assert
            Assert.IsNotNull(jobAssignment);
            Assert.AreEqual(jobAssignment.Title, "Title Update");
            Assert.AreEqual(jobAssignment.StartDate, DateTime.Now.Date.AddDays(-200));
            Assert.AreEqual(jobAssignment.EndDate, DateTime.Now.Date);
            Assert.AreEqual(jobAssignment.CompLevel, 60);
            Assert.AreEqual(jobAssignment.OutcomeCode, 2);

        }
    }
}
