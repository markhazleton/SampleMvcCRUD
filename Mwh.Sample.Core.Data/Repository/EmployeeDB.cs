using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Core.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.Sample.Core.Data.Repository
{
    public class EmployeeDB : IEmployeeDB
    {
        Models.EmployeeContext _context;
        public EmployeeDB(Models.EmployeeContext context)
        {
            _context = context;
        }
        private List<EmployeeModel> Create(List<Models.Employee> list)
        {
            if (list == null) return new List<EmployeeModel>();
            return list.Select(item => Create(item)).OrderBy(x => x.Name).ToList();
        }
        private EmployeeModel Create(Models.Employee s)
        {
            if (s == null) return new EmployeeModel();

            return new EmployeeModel()
            {
                EmployeeID = s.EmployeeId,
                State = s.State,
                Age = s.Age,
                Country = s.Country,
                Department = (EmployeeDepartment)s.DepartmentId,
                Name = s.Name
            };
        }

        public bool Delete(int ID)
        {
            var delEmployee = _context.Employees.Where(w => w.EmployeeId == ID).FirstOrDefault();
            if (delEmployee != null)
            {
                _context.Employees.Remove(delEmployee);
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public EmployeeModel Employee(int id)
        {
            return Create(_context.Employees.Where(w => w.EmployeeId == id).FirstOrDefault());
        }

        public List<EmployeeModel> EmployeeCollection()
        {
            return Create(_context.Employees.OrderBy(o => o.Name).ToList());
        }

        public EmployeeModel Update(EmployeeModel emp)
        {
            if (emp == null) return null;

            if (emp.EmployeeID == 0)
            {
                var saveUser = new Employee()
                {
                    Name = emp.Name,
                    State = emp.State,
                    Age = emp.Age,
                    Country = emp.Country,
                    DepartmentId = (int)emp.Department
                };
                _context.Employees.Add(saveUser);
                _context.SaveChanges();
                emp.EmployeeID = saveUser.EmployeeId;
            }
            else
            {
                var saveUser = _context.Employees.Where(w => w.EmployeeId == emp.EmployeeID).FirstOrDefault();
                if (saveUser != null)
                {

                    _context.Attach(saveUser);
                    saveUser.Name = emp.Name;
                    saveUser.State = emp.State;
                    saveUser.Age = emp.Age;
                    saveUser.Country = emp.Country;
                    saveUser.DepartmentId = (int)emp.Department;
                    _context.SaveChanges();
                }
            }
            return Employee(emp.EmployeeID);
        }
    }
}
