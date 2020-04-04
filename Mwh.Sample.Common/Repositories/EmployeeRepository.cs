using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IEmployeeDB _emp;

        public EmployeeRepository(IEmployeeDB employeeDB) { _emp = employeeDB; }

        public async Task AddAsync(EmployeeModel employee, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await Task.Run(() => _emp.Update(employee)).ConfigureAwait(true);
        }

        public Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => _emp.Employee(id));
        }

        public Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => (IEnumerable<EmployeeModel>)_emp.EmployeeCollection());
        }

        public void Remove(EmployeeModel employee) { _emp.Delete(employee.EmployeeID); }

        public void Update(EmployeeModel employee) { _emp.Update(employee); }
    }
}
