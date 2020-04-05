using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token);
        Task AddAsync(EmployeeModel employee, CancellationToken token);
        Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token);
        EmployeeModel Update(EmployeeModel employee);
        bool Remove(EmployeeModel employee);
    }
}
