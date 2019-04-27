namespace SampleCRUD.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class EmployeeMock : IEmployeeDB
    {
        private List<Employee> _list;

        public EmployeeMock()
        {
            _list = new List<Employee>()
            {
                new Employee() { Name="Mark", Age = 50, State = "Texas", Country = "USA", EmployeeID=1 },
                new Employee() { Name="Alan", Age = 53, State = "Texas", Country = "USA", EmployeeID=2 },
                new Employee() { Name="Lesley", Age = 50, State = "Texas", Country = "USA", EmployeeID=3 },
            };


            foreach (var emp in _list)
            {
                emp.JobList.Add(new JobAssignment()
                {
                    CompLevel = 1,
                    StartDate = DateTime.Now.Date.AddDays(-100),
                    EndDate = DateTime.Now.Date.AddDays(-50),
                    OutcomeCode = 2,
                    Title = "Solution Architect"
                });
                emp.JobList.Add(new JobAssignment()
                {
                    CompLevel = 2,
                    StartDate = DateTime.Now.Date.AddDays(-49),
                    EndDate = DateTime.Now.Date.AddDays(-1),
                    OutcomeCode = 3,
                    Title = "Developer"
                });
            }


        }

        //Method for Deleting an Employee
        public int Delete(int ID)
        {
            var myEmp = _list.Where(w => w.EmployeeID == ID).FirstOrDefault();
            if (myEmp == null) return -1;
            _list.Remove(myEmp);
            return 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Employee Get(int id)
        {
            var myEmp = _list.Where(w => w.EmployeeID == id).FirstOrDefault();
            if (myEmp == null) return new Employee();
            return myEmp;
        }

        //Return list of all Employees
        public List<Employee> ListAll()
        {
            return _list;
        }
        //Method for Updating Employee record
        /// <summary>
        /// 
        /// </summary>
        /// <param name="emp"></param>
        /// <returns></returns>
        public int Update(Employee emp)
        {
            if (emp.EmployeeID == 0)
            {
                int nextID = _list.OrderByDescending(o => o.EmployeeID).Select(s => s.EmployeeID).FirstOrDefault() + 1;
                emp.EmployeeID = nextID;
                _list.Add(emp);
                return nextID;
            }
            else
            {
                var myEmp = _list.Where(w => w.EmployeeID == emp.EmployeeID).FirstOrDefault();

                if (myEmp == null) return -1;

                myEmp.Name = emp.Name;
                myEmp.Age = emp.Age;
                myEmp.Country = emp.Country;
                myEmp.State = emp.State;
                return myEmp.EmployeeID;
            }

        }
    }
}
