using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Repositories
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token);
        Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token);
        Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token);
        Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token);
    }
}
