using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using System.Linq;

namespace Mwh.Sample.Common.Repositories
{
    [TestClass]
    public class EmployeeMockTests
    {
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

        [TestMethod()]
        public void DeleteTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.EmployeeCollection().FirstOrDefault();
            var count = Employee.EmployeeCollection().Count;

            Employee.Delete(myEmp.EmployeeID);
            Assert.AreEqual(count - 1, Employee.EmployeeCollection().Count);
        }

        [TestMethod()]
        public void DeleteTestEmployeeIDZero()
        {
            var Employee = new EmployeeMock();
            var count = Employee.EmployeeCollection().Count;
            Employee.Delete(0);
            Assert.AreEqual(count, Employee.EmployeeCollection().Count);
        }

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


        [TestMethod()]
        public void EmployeeMockTest()
        {
            var Employee = new EmployeeMock();
            Assert.AreNotEqual(0, Employee.EmployeeCollection().Count);
        }

        [TestMethod()]
        public void GetTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.EmployeeCollection().FirstOrDefault();
            Employee.Employee(myEmp.EmployeeID);
            Assert.AreEqual(myEmp.Name, Employee.Employee(myEmp.EmployeeID).Name);
        }

        [TestMethod()]
        public void GetTestEmployeeIDZero()
        {
            var Employee = new EmployeeMock();
            Assert.AreEqual(null, Employee.Employee(0).Name);
        }

        [TestMethod()]
        public void ListAllTest()
        {
            var Employee = new EmployeeMock();
            Assert.AreNotEqual(0, Employee.EmployeeCollection().Count);
        }

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

        [TestMethod()]
        public void UpdateTestInvalidEmployeeID()
        {
            var Employee = new EmployeeMock();
            var myEmp = new EmployeeModel { EmployeeID = 99999 };
            Assert.AreEqual(99999, Employee.Update(myEmp).EmployeeID);
        }

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
