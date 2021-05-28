using Microsoft.EntityFrameworkCore;
using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Models;
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
            if (item == null) return new EmployeeModel();

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

            if (namelist == null) return -1;

            foreach (var name in namelist)
            {
                list.Add(new Employee() { Name = name, Age = 33, Country = "USA", DepartmentId = 1, State = "TX" });
            }
            _context.Employees.AddRange(list);
            var dbResult = _context.SaveChanges();
            return dbResult;
        }

        public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            var response = new EmployeeResponse("Init");

            if (id > 0)
            {
                var dbEmp = await _context.Employees
                    .Where(w => w.EmployeeId == id)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(true);

                if (dbEmp == null)
                {
                    response = new EmployeeResponse("Employee Not Found");
                }
                else
                {
                    _context.Employees.Remove(dbEmp);
                    await _context.SaveChangesAsync()
                        .ConfigureAwait(true);

                    return new EmployeeResponse(new EmployeeModel());
                }
            }
            else
            {
                return new EmployeeResponse("Invalid Employee Id for delete");
            }
            return response;
        }

        public void Dispose() { ((IDisposable)_context).Dispose(); }

        public async Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        { return Create(await _context.Employees.FindAsync(id).ConfigureAwait(false)); }

        public EmployeeModel[] GetEmployeeCollection() { return CreateCollection(_context.Employees.ToList()); }

        public async Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        { return CreateCollection(await _context.Employees.ToListAsync().ConfigureAwait(false)); }

        public async Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            if (employee == null) return new EmployeeResponse("Employee can not be null");

            var emp = await SaveEmployeeAsync(employee).ConfigureAwait(true);
            return emp;
        }

        public EmployeeResponse SaveEmployee(EmployeeModel item)
        {
            if (item == null)
                return new EmployeeResponse("Employee can not be null");

            Models.Employee dbEmp;
            if (item.EmployeeID > 0)
            {
                dbEmp = _context.Employees.Where(w => w.EmployeeId == item.EmployeeID).FirstOrDefault();
                if (dbEmp == null)
                {
                    return new EmployeeResponse("Employee Not Found");
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
            return new EmployeeResponse(Create(dbEmp));
        }

        public async Task<EmployeeResponse> SaveEmployeeAsync(EmployeeModel item)
        {
            if (item == null)
                return new EmployeeResponse("Employee can not be null");

            var dbEmp = new Employee();
            try
            {
                if (item.EmployeeID > 0)
                {
                    dbEmp = await _context.Employees
                        .Where(w => w.EmployeeId == item.EmployeeID)
                        .FirstOrDefaultAsync()
                        .ConfigureAwait(true);

                    if (dbEmp == null)
                    {
                        return new EmployeeResponse("Employee Not Found");
                    }
                    else
                    {
                        dbEmp.Age = item.Age;
                        dbEmp.Country = item.Country;
                        dbEmp.DepartmentId = (int)item.Department;
                        dbEmp.Name = item.Name;
                        dbEmp.State = item.State;
                        await _context.SaveChangesAsync()
                            .ConfigureAwait(true);

                        var list = await _context.Employees
                            .ToListAsync()
                            .ConfigureAwait(true);
                    }
                }
                else
                {
                    dbEmp = Create(item);
                    await _context.Employees.AddAsync(dbEmp);
                    await _context.SaveChangesAsync()
                        .ConfigureAwait(true);
                }
            }
            catch (Exception ex)
            {
                return new EmployeeResponse($"Exception:{ex.Message}");

            }

            var newEmp = await _context.Employees
                .Where(w => w.EmployeeId == dbEmp.EmployeeId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(true);

            return new EmployeeResponse(Create(dbEmp));
        }

        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            if (employee ==null)
                return new EmployeeResponse($"Can not update null employee");

            if (employee.EmployeeID != id)
                return new EmployeeResponse($"Mismatch in id({id}) && employee_id({employee.EmployeeID}).");

            if (employee.EmployeeID == 0)
                return new EmployeeResponse($"Can not update employee with id({id})");

            return await SaveEmployeeAsync(employee).ConfigureAwait(false);
        }
    }
}
