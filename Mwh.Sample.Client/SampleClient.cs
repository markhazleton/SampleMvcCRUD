using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Client
{
    public class SampleClient : ClientBase, IEmployeeService
    {
        public SampleClient(string apiPath, string appName) : base(apiPath, appName)
        {

        }
        public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            var resp = await Delete<EmployeeResponse>($"/api/employee/{id}").ConfigureAwait(true);
            return resp;
        }

        public async Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        {
            return await Get<EmployeeModel>($"/api/employee/{id}").ConfigureAwait(true);
        }

        public async Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        {
            return await Get<List<EmployeeModel>>($"/api/employee").ConfigureAwait(true);
        }

        public async Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            var resp = await Post<EmployeeResponse>($"/api/employee", employee).ConfigureAwait(true);
            return resp;
        }

        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            var resp = await Put<EmployeeResponse>($"/api/employee/{id}", employee).ConfigureAwait(true);
            return resp;
        }
    }
}
