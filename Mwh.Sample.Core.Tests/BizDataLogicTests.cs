using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.Sample.Core.Tests
{
    [TestClass]
    public class BizDataLogicTests
    {
        [TestMethod]
        public void AddMultipleEmployeesReturnsCorrectNumberOfInsertRows()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("AddMultipleEmployees");

            using (var bizlogic = new BusinessDataLogic(builder.Options))
            {
                var employeeNames = new string[] { "Lesley", "Mark", "Marlis", "Berit", "Ian" };
                var newEmployeesCreated = bizlogic.AddMultipleEmployees(employeeNames);
                Assert.AreEqual(employeeNames.Length, newEmployeesCreated);
            }
        }

        [TestMethod]
        public void GetEmployeeCollection()
        {
            var employeeNames = new string[] { "Lesley", "Mark", "Marlis", "Berit", "Ian" };
            int newEmployeesCreated;
            EmployeeModel emp;
            EmployeeModel emp2;

            List<EmployeeModel> list = new List<EmployeeModel>();
            var builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("GetEmployeeCollectionAsync");
            builder.EnableSensitiveDataLogging();

            using (var bizlogic = new BusinessDataLogic(builder.Options))
            {
                newEmployeesCreated = bizlogic.AddMultipleEmployees(employeeNames);
                list.AddRange(bizlogic.GetEmployeeCollection());

                emp = list.Where(w => w.EmployeeID == 1).FirstOrDefault();

                emp.Name = "Test Mark";
                emp2 = bizlogic.SaveEmployee(emp);

                list.Clear();
                list.AddRange(bizlogic.GetEmployeeCollection());

            }
            Assert.AreEqual(employeeNames.Length, list.Count);
            Assert.AreEqual(emp.Name, "Test Mark");
        }


        [TestMethod]
        public void CanInsertEmployeeIntoDatabase()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("CanInsertEmployee");
            builder.EnableSensitiveDataLogging();

            using (BusinessDataLogic context = new BusinessDataLogic(builder.Options))
            {
                var employee = new EmployeeModel()
                {
                    Name = "Test",
                    Age = 33,
                    State = "Texas",
                    Country = "USA",
                    Department = EmployeeDepartment.Accounting
                };

                employee = context.SaveEmployee(employee);

                Assert.AreNotEqual(0, employee.EmployeeID);
            }
        }

    }
}
