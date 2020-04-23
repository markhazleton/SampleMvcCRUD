using Microsoft.EntityFrameworkCore;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using Mwh.Sample.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.Data
{
    public class BusinessDataLogic : IDisposable, IEmployeeService
    {
        private SampleContext _context;
        public BusinessDataLogic() { _context = new SampleContext(); }
        public BusinessDataLogic(SampleContext context) { _context = context; }
        public BusinessDataLogic(DbContextOptions options) { _context = new SampleContext(options); }

        public int AddMultipleEmployees(string[] namelist)
        {
            var list = new List<Employee>();
            foreach (var name in namelist)
            {
                list.Add(new Employee() { Name = name, Age = 33, Country = "USA", DepartmentId = 1, State = "TX" });
            }
            _context.Employees.AddRange(list);
            var dbResult = _context.SaveChanges();
            return dbResult;
        }

        public Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        { throw new NotImplementedException(); }

        public void Dispose() { ((IDisposable)_context).Dispose(); }

        public async Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        { return Create(await _context.Employees.FindAsync(id).ConfigureAwait(true)); }

        public EmployeeModel[] GetEmployeeCollection() { return CreateCollection(_context.Employees.ToList()); }

        public async Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        { return CreateCollection(await _context.Employees.ToListAsync().ConfigureAwait(true)); }

        public async Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            var emp = SaveEmployee(employee);
            return new EmployeeResponse(emp);
        }

        public EmployeeModel SaveEmployee(EmployeeModel item)
        {
            Employee dbEmp;
            if (item.EmployeeID > 0)
            {
                dbEmp = _context.Employees.Where(w => w.EmployeeId == item.EmployeeID).FirstOrDefault();
                if (dbEmp == null)
                {
                    return item;
                }
                else
                {
                    dbEmp.Age = item.Age;
                    dbEmp.Country = item.Country;
                    dbEmp.DepartmentId = (int)item.Department;
                    dbEmp.Name = item.Name;
                    dbEmp.State = item.State;
                    _context.SaveChanges();
                }
            }
            else
            {
                dbEmp = Create(item);
                _context.Employees.Add(dbEmp);
                _context.SaveChanges();
            }
            return Create(dbEmp);
        }

        public async Task<EmployeeModel> SaveEmployeeAsync(EmployeeModel item)
        {
            Employee dbEmp = new Employee();
            try
            {
                if (item.EmployeeID > 0)
                {
                    dbEmp =  _context.Employees
                        .Where(w => w.EmployeeId == item.EmployeeID)
                        .FirstOrDefault();

                    if (dbEmp == null)
                    {
                        return item;
                    }
                    else
                    {
                        dbEmp.Age = item.Age;
                        dbEmp.Country = item.Country;
                        dbEmp.DepartmentId = (int)item.Department;
                        dbEmp.Name = item.Name;
                        dbEmp.State = item.State;
                        _context.SaveChanges();

                        var list = _context.Employees.ToList();

                    }
                }
                else
                {
                    dbEmp = Create(item);
                    _context.Employees.Add(dbEmp);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                
            }

            var newEmp = await _context.Employees.Where(w => w.EmployeeId == dbEmp.EmployeeId).FirstOrDefaultAsync().ConfigureAwait(true);

            return Create(newEmp);
        }

        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            var emp = await SaveEmployeeAsync(employee).ConfigureAwait(true);
            return new EmployeeResponse(emp);
        }

        private EmployeeModel Create(Employee item)
        {
            return new EmployeeModel()
            {
                Name = item.Name,
                State = item.State,
                Country = item.Country,
                Age = item.Age,
                Department = (EmployeeDepartment)item.DepartmentId,
                EmployeeID = item.EmployeeId
            };
        }
        private Employee Create(EmployeeModel item)
        {
            return new Employee()
            {
                Name = item.Name,
                State = item.State,
                Country = item.Country,
                Age = item.Age,
                DepartmentId = (int)item.Department,
                EmployeeId = item.EmployeeID
            };
        }

        private EmployeeModel[] CreateCollection(List<Employee> list) { return list.Select(s => Create(s)).ToArray(); }
        private Employee[] CreateCollection(List<EmployeeModel> list) { return list.Select(s => Create(s)).ToArray(); }
    }
}
