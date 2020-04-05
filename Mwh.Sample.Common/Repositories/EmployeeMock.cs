using Mwh.Sample.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Employee Mock Repository
    /// </summary>
    public class EmployeeMock : IEmployeeDB
    {
        private List<EmployeeModel> _list;

        /// <summary>
        /// Constructor
        /// </summary>
        public EmployeeMock()
        {
            _list = new List<EmployeeModel>()
            {
                new EmployeeModel()
                {
                    Name = "Jim",
                    Age = 35,
                    Department = EmployeeDepartment.IT,
                    State = "Florida",
                    Country = "USA",
                    EmployeeID = 4
                },
                new EmployeeModel()
                {
                    Name = "Bob",
                    Age = 50,
                    Department = EmployeeDepartment.IT,
                    State = "Texas",
                    Country = "USA",
                    EmployeeID = 1
                },
                new EmployeeModel()
                {
                    Name = "Sam",
                    Age = 53,
                    Department = EmployeeDepartment.Marketing,
                    State = "Texas",
                    Country = "USA",
                    EmployeeID = 2
                },
                new EmployeeModel()
                {
                    Name = "Frank",
                    Age = 50,
                    Department = EmployeeDepartment.Executive,
                    State = "Texas",
                    Country = "USA",
                    EmployeeID = 3
                },
            };


            foreach (var emp in _list)
            {
                if (emp == null)
                    continue;

                emp.JobList
                    .Add(new JobAssignmentModel()
                    {
                        CompLevel = 1,
                        StartDate = DateTime.Now.Date.AddDays(-100),
                        EndDate = DateTime.Now.Date.AddDays(-50),
                        OutcomeCode = 2,
                        Title = "Solution Architect"
                    });
                emp.JobList
                    .Add(new JobAssignmentModel()
                    {
                        CompLevel = 2,
                        StartDate = DateTime.Now.Date.AddDays(-49),
                        EndDate = DateTime.Now.Date.AddDays(-1),
                        OutcomeCode = 3,
                        Title = "Developer"
                    });
            }
        }

        public static EmployeeMock Instance { get; } = new EmployeeMock();

        /// <summary>
        /// Method for Deleting an Employee
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool Delete(int ID)
        {
            var myEmp = _list.Where(w => w.EmployeeID == ID).FirstOrDefault();
            if (myEmp == null)
                return false;
            if (_list.Remove(myEmp))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EmployeeModel Employee(int id)
        {
            var myEmp = _list.Where(w => w.EmployeeID == id).FirstOrDefault();
            if (myEmp == null)
                return new EmployeeModel();
            return myEmp;
        }

        //Return list of all Employees
        public List<EmployeeModel> EmployeeCollection() { return _list; }

        //Method for Updating Employee record
        /// <summary>
        ///
        /// </summary>
        /// <param name="emp"></param>
        /// <returns></returns>
        public EmployeeModel Update(EmployeeModel emp)
        {
            if (emp == null)
                return new EmployeeModel();

            if (!emp.IsValid)
                return emp;

            if (emp.EmployeeID == 0)
            {
                int nextID = _list.OrderByDescending(o => o.EmployeeID).Select(s => s.EmployeeID).FirstOrDefault() + 1;
                emp.EmployeeID = nextID;
                _list.Add(emp);
                return emp;
            }
            else
            {
                var myEmp = _list.Where(w => w.EmployeeID == emp.EmployeeID).FirstOrDefault();

                if (myEmp == null)
                    return new EmployeeModel();

                myEmp.Name = emp.Name;
                myEmp.Age = emp.Age;
                myEmp.Department = emp.Department;
                myEmp.Country = emp.Country;
                myEmp.State = emp.State;
                return myEmp;
            }
        }
    }
}
