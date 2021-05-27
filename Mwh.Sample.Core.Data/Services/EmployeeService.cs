using Microsoft.EntityFrameworkCore;
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using Mwh.Sample.Core.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.Data.Services
{
    public class EmployeeService : IDisposable, IEmployeeService
    {
        private Models.EmployeeContext _context;

        public EmployeeService() { _context = new EmployeeContext(); }
        public EmployeeService(Models.EmployeeContext context) { _context = context; }
        public EmployeeService(DbContextOptions options) { _context = new EmployeeContext(options); }

        private EmployeeModel Create(Models.Employee item)
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
        private Models.Employee Create(EmployeeModel item)
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

        private EmployeeModel[] CreateCollection(List<Models.Employee> list) { return list.Select(s => Create(s)).ToArray(); }
        private Models.Employee[] CreateCollection(List<EmployeeModel> list) { return list.Select(s => Create(s)).ToArray(); }

        public int AddMultipleEmployees(string[] namelist)
        {
            var list = new List<Models.Employee>();
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
        { return Create(await _context.Employees.FindAsync(id).ConfigureAwait(false)); }

        public EmployeeModel[] GetEmployeeCollection() { return CreateCollection(_context.Employees.ToList()); }

        public async Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        { return CreateCollection(await _context.Employees.ToListAsync().ConfigureAwait(false)); }

        public async Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            var emp = SaveEmployee(employee);
            return new EmployeeResponse(emp);
        }

        public EmployeeModel SaveEmployee(EmployeeModel item)
        {
            Models.Employee dbEmp;
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
            Models.Employee dbEmp = new Employee();
            try
            {
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

            var newEmp = await _context.Employees
                .Where(w => w.EmployeeId == dbEmp.EmployeeId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(true);

            return Create(newEmp);
        }

        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            if (employee.EmployeeID != id)
                return new EmployeeResponse($"Mismatch in id({id}) && employee_id({employee.EmployeeID}).");

            var emp = await SaveEmployeeAsync(employee).ConfigureAwait(false);
            return new EmployeeResponse(emp);
        }
    }
}
