using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace Mwh.SampleCRUD.BL.Repositories
{
    [TestClass()]
    public class EmployeeMockTests
    {
        [TestMethod()]
        public void EmployeeMockTest()
        {
            var Employee = new EmployeeMock();
            Assert.AreNotEqual(0, Employee.EmployeeCollection().Count);
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
            Assert.AreEqual(-1, Employee.Update(myEmp));
        }

        [TestMethod()]
        public void UpdateTestNewEmployee()
        {
            var Employee = new EmployeeMock();
            var count = Employee.EmployeeCollection().Count;
            var myEmp = Employee.EmployeeCollection().FirstOrDefault();
            var NewName = "NewName";
            myEmp.Name = NewName;
            myEmp.EmployeeID = Employee.Update(myEmp);
            Assert.AreEqual(NewName, Employee.Employee(myEmp.EmployeeID).Name);
            Assert.AreEqual(count, Employee.EmployeeCollection().Count);
        }
    }
}