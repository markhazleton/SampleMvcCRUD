// ***********************************************************************
// Assembly         : Mwh.Sample.Common.Tests
// Author           : mark
// Created          : 04-04-2020
//
// Last Modified By : mark
// Last Modified On : 04-05-2020
// ***********************************************************************
// <copyright file="EmployeeMockTests.cs" company="Mwh.Sample.Common.Tests">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using System.Linq;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Defines test class EmployeeMockTests.
    /// </summary>
    [TestClass]
    public class EmployeeMockTests
    {
        /// <summary>
        /// Defines the test method DeleteExpectedBehavior.
        /// </summary>
        [TestMethod]
        public void DeleteExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            int ID = 0;

            // Act
            var result = employeeMock.Delete(ID);

            // Assert
            Assert.AreEqual(result, false);
        }

        /// <summary>
        /// Defines the test method DeleteTest.
        /// </summary>
        [TestMethod()]
        public void DeleteTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.EmployeeCollection().FirstOrDefault();
            var count = Employee.EmployeeCollection().Count;

            Employee.Delete(myEmp.EmployeeID);
            Assert.AreEqual(count - 1, Employee.EmployeeCollection().Count);
        }

        /// <summary>
        /// Defines the test method DeleteTestEmployeeIDZero.
        /// </summary>
        [TestMethod()]
        public void DeleteTestEmployeeIDZero()
        {
            var Employee = new EmployeeMock();
            var count = Employee.EmployeeCollection().Count;
            Employee.Delete(0);
            Assert.AreEqual(count, Employee.EmployeeCollection().Count);
        }

        /// <summary>
        /// Defines the test method EmployeeCollectionExpectedBehavior.
        /// </summary>
        [TestMethod]
        public void EmployeeCollectionExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();

            // Act
            var result = employeeMock.EmployeeCollection();

            // Assert
            Assert.AreNotEqual(result.Count, 0);
        }

        /// <summary>
        /// Defines the test method EmployeeExpectedBehavior.
        /// </summary>
        [TestMethod]
        public void EmployeeExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            int id = 0;

            // Act
            var result = employeeMock.Employee(id);

            // Assert
            Assert.IsFalse(result.IsValid);
        }


        /// <summary>
        /// Defines the test method EmployeeMockTest.
        /// </summary>
        [TestMethod()]
        public void EmployeeMockTest()
        {
            var Employee = new EmployeeMock();
            Assert.AreNotEqual(0, Employee.EmployeeCollection().Count);
        }

        /// <summary>
        /// Defines the test method GetTest.
        /// </summary>
        [TestMethod()]
        public void GetTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.EmployeeCollection().FirstOrDefault();
            Employee.Employee(myEmp.EmployeeID);
            Assert.AreEqual(myEmp.Name, Employee.Employee(myEmp.EmployeeID).Name);
        }

        /// <summary>
        /// Defines the test method GetTestEmployeeIDZero.
        /// </summary>
        [TestMethod()]
        public void GetTestEmployeeIDZero()
        {
            var Employee = new EmployeeMock();
            Assert.AreEqual(null, Employee.Employee(0).Name);
        }

        /// <summary>
        /// Defines the test method ListAllTest.
        /// </summary>
        [TestMethod()]
        public void ListAllTest()
        {
            var Employee = new EmployeeMock();
            Assert.AreNotEqual(0, Employee.EmployeeCollection().Count);
        }

        /// <summary>
        /// Defines the test method UpdateExpectedBehavior.
        /// </summary>
        [TestMethod]
        public void UpdateExpectedBehavior()
        {
            // Arrange
            var employeeMock = new EmployeeMock();
            EmployeeModel emp = null;

            // Act
            var result = employeeMock.Update(emp);

            // Assert
            Assert.AreEqual(result.EmployeeID, 0);
        }

        /// <summary>
        /// Defines the test method UpdateTest.
        /// </summary>
        [TestMethod()]
        public void UpdateTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.EmployeeCollection().FirstOrDefault();
            var NewName = "NewName";
            myEmp.Name = NewName;
            Employee.Update(myEmp);
            Assert.AreEqual(NewName, Employee.Employee(myEmp.EmployeeID).Name);
        }

        /// <summary>
        /// Defines the test method UpdateTestInvalidEmployeeID.
        /// </summary>
        [TestMethod()]
        public void UpdateTestInvalidEmployeeID()
        {
            var Employee = new EmployeeMock();
            var myEmp = new EmployeeModel { EmployeeID = 99999 };
            Assert.AreEqual(99999, Employee.Update(myEmp).EmployeeID);
        }

        /// <summary>
        /// Defines the test method UpdateTestNewEmployee.
        /// </summary>
        [TestMethod()]
        public void UpdateTestNewEmployee()
        {
            var Employee = new EmployeeMock();
            var count = Employee.EmployeeCollection().Count;
            var myEmp = Employee.EmployeeCollection().FirstOrDefault();
            var NewName = "NewName";
            myEmp.Name = NewName;
            myEmp = Employee.Update(myEmp);
            Assert.AreEqual(NewName, Employee.Employee(myEmp.EmployeeID).Name);
            Assert.AreEqual(count, Employee.EmployeeCollection().Count);
        }
    }
}
