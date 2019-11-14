using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.SampleCRUD.BL.Models;
using Mwh.SampleCRUD.BL.Repositories;
using System;
using System.Linq;

namespace Mwh.SampleCRUD.BL.Tests.Repositories
{
    [TestClass()]
    public class EmployeeMockTests
    {
        [TestMethod()]
        public void EmployeeMockTest()
        {
            var Employee = new EmployeeMock();
            Assert.AreEqual(3, Employee.ListAll().Count);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.ListAll().FirstOrDefault();
            Employee.Delete(myEmp.EmployeeID);
            Assert.AreEqual(2, Employee.ListAll().Count);
        }

        [TestMethod()]
        public void DeleteTestEmployeeIDZero()
        {
            var Employee = new EmployeeMock();
            Employee.Delete(0);
            Assert.AreEqual(3, Employee.ListAll().Count);
        }

        [TestMethod()]
        public void GetTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.ListAll().FirstOrDefault();
            Employee.Get(myEmp.EmployeeID);
            Assert.AreEqual(myEmp.Name, Employee.Get(myEmp.EmployeeID).Name);
        }

        [TestMethod()]
        public void GetTestEmployeeIDZero()
        {
            var Employee = new EmployeeMock();
            Assert.AreEqual(null, Employee.Get(0).Name);
        }

        [TestMethod()]
        public void ListAllTest()
        {
            var Employee = new EmployeeMock();
            Assert.AreEqual(3, Employee.ListAll().Count);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.ListAll().FirstOrDefault();
            var NewName = "NewName";
            myEmp.Name = NewName;
            Employee.Update(myEmp);
            Assert.AreEqual(NewName, Employee.Get(myEmp.EmployeeID).Name);
        }

        [TestMethod()]
        public void UpdateTestInvalidEmployeeID()
        {
            var Employee = new EmployeeMock();
            var myEmp = new EmployeeModel { EmployeeID = 99999 };
            Assert.AreEqual(-1, Employee.Update(myEmp));
        }

        [TestMethod()]
        public void UpdateTestNewEmployee()
        {
            var Employee = new EmployeeMock();
            var myEmp = Employee.ListAll().FirstOrDefault();
            var NewName = "NewName";
            myEmp.Name = NewName;
            myEmp.EmployeeID = 0;
            myEmp.EmployeeID = Employee.Update(myEmp);
            Assert.AreEqual(NewName, Employee.Get(myEmp.EmployeeID).Name);
            Assert.AreEqual(4, Employee.ListAll().Count);
        }
    }
}