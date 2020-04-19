// ***********************************************************************
// Assembly         : Mwh.Sample.Common.Tests
// Author           : mark
// Created          : 04-04-2020
//
// Last Modified By : mark
// Last Modified On : 04-04-2020
// ***********************************************************************
// <copyright file="JobAssignmentModelTests.cs" company="Mwh.Sample.Common.Tests">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Defines test class JobAssignmentModelTests.
    /// </summary>
    [TestClass]
    public class JobAssignmentModelTests
    {
        /// <summary>
        /// Defines the test method JobAssignmentModelTest.
        /// </summary>
        [TestMethod]
        public void JobAssignmentModelTest()
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
