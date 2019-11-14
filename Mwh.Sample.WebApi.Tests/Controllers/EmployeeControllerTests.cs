using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.WebApi.Controllers;
using Mwh.SampleCRUD.BL.Models;
using System;

namespace Mwh.Sample.WebApi.Tests.Controllers
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private EmployeeController controller;

        [TestInitialize]
        public void TestInitialize()
        {
            controller = new EmployeeController();
        }

        [TestMethod]
        public void Add_StateUnderTest_NullBehavior()
        {
            // Arrange
            EmployeeModel emp = null;

            // Act
            var result = controller.Add(emp);
            int data = 0;
            Int32.TryParse(result.Data.ToString(), out data);
            // Assert
            Assert.AreEqual(-1, data);
        }

        [TestMethod]
        public void Add_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var emp = new EmployeeModel()
            {
                EmployeeID = 0,
                Age = 25,
                Name = "Bill",
                Country = "USA",
                State = "Texas",
                Department = EmployeeDepartment.IT,
                JobList = new JobAssignmentList()
            };

            // Act
            var result = controller.Add(emp);
            int data = 0;
            Int32.TryParse(result.Data.ToString(), out data);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(0< data);
            Assert.AreEqual(emp.Country, "USA");
            Assert.AreEqual(emp.Name, "Bill");
            Assert.AreEqual(emp.State, "Texas");
            Assert.AreEqual(emp.Department, EmployeeDepartment.IT);
            Assert.AreEqual(emp.Age, 25);



        }

        [TestMethod]
        public void Delete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = controller.Delete(id);
            int data = 0;
            Int32.TryParse(result.Data.ToString(), out data);
            // Assert
            Assert.AreEqual(-1, data);
        }

        [TestMethod]
        public void EmpSinglePage_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.EmpSinglePage();

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetbyID_StateUnderTest_IdIsZero()
        {
            // Arrange
            int id = 0;

            // Act
            var result = controller.GetbyID(id);
            var emp = (EmployeeModel)result.Data;

            result = controller.GetbyID(id);
            var emp2 = (EmployeeModel)result.Data;

            // Assert
            Assert.AreEqual(emp.EmployeeID, emp2.EmployeeID);
            Assert.AreEqual(0, emp.EmployeeID);
        }

        [TestMethod]
        public void GetbyID_StateUnderTest_ExpectedBehaviorFromCache()
        {
            // Arrange
            int id = 3;

            // Act
            var result = controller.GetbyID(id);
            var emp = (EmployeeModel)result.Data;

            result = controller.GetbyID(id);
            var emp2 = (EmployeeModel)result.Data;

            // Assert
            Assert.AreEqual(emp.EmployeeID, emp2.EmployeeID);
        }

        [TestMethod]
        public void GetEmployeeDelete_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = controller.GetEmployeeDelete(id);

            // Assert
        }

        [TestMethod]
        public void GetEmployeeEdit_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            int id = 0;

            // Act
            var result = controller.GetEmployeeEdit(id);

            // Assert
        }

        [TestMethod]
        public void GetEmployeeList_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.GetEmployeeList();

            // Assert
        }

        [TestMethod]
        public void Index_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.Index();

            // Assert
        }

        [TestMethod]
        public void List_StateUnderTest_ExpectedBehavior()
        {
            // Arrange

            // Act
            var result = controller.List();

            // Assert
        }

        [TestMethod]
        public void Save_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeModel postEmployee = null;

            // Act
            var result = controller.Save(postEmployee);

            // Assert
        }

        [TestMethod]
        public void Update_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            EmployeeModel emp = null;

            // Act
            var result = controller.Update(emp);

            // Assert
        }
    }
}
