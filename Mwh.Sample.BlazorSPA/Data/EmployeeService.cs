using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.Sample.BlazorSPA.Data
{
    public class EmployeeService : IEmployeeService
    {
        private readonly SqlDbContext _dbContext;

        public EmployeeService(SqlDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Employee>> GetEmployees()
        {
            return await _dbContext.Employees.ToListAsync();
        }
        public async Task<bool> CreateEmployee(Employee employee)
        {
            employee.Id = Guid.NewGuid().ToString();
            _dbContext.Add(employee);
            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }

        }
        public async Task<Employee> SingleEmployee(string id)
        {
            return await _dbContext.Employees.FindAsync(id);
        }
        public async Task<bool> EditEmployee(string id, Employee employee)
        {
            if (id != employee.Id)
            {
                return false;
            }

            _dbContext.Entry(employee).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteEmployee(string id)
        {
            var patient = await _dbContext.Employees.FindAsync(id);
            if (patient == null)
            {
                return false;
            }

            _dbContext.Employees.Remove(patient);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
