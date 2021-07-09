using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using System.Linq;

namespace Mwh.Sample.Common.Tests.Repositories
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

            Employee.Delete(myEmp.id);
            Assert.AreEqual(count - 1, Employee.EmployeeCollection().Count);
        }

        /// <summary>
        /// Defines the test method DeleteTestIDZero.
        /// </summary>
        [TestMethod()]
        public void DeleteTestIDZero()
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
            Employee.Employee(myEmp.id);
            Assert.AreEqual(myEmp.Name, Employee.Employee(myEmp.id).Name);
        }

        /// <summary>
        /// Defines the test method GetTestIDZero.
        /// </summary>
        [TestMethod()]
        public void GetTestIDZero()
        {
            var Employee = new EmployeeMock();
            Assert.AreEqual(null, Employee.Employee(0)?.Name);
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
            Assert.AreEqual(result.id, 0);
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
            Assert.AreEqual(NewName, Employee.Employee(myEmp.id).Name);
        }

        /// <summary>
        /// Defines the test method UpdateTestInvalidId.
        /// </summary>
        [TestMethod()]
        public void UpdateTestInvalidId()
        {
            var Employee = new EmployeeMock();
            var myEmp = new EmployeeModel { id = 99999 };
            Assert.AreEqual(99999, Employee.Update(myEmp).id);
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
            Assert.AreEqual(NewName, Employee.Employee(myEmp.id).Name);
            Assert.AreEqual(count, Employee.EmployeeCollection().Count);
        }
    }
}