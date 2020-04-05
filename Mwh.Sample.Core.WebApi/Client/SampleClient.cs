using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Core.WebApi
{
    public class SampleClient : ClientBase, IEmployeeService
    {
        public SampleClient(string apiPath,string appName):base(apiPath,appName)
        { 

        }
        public Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public async Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        {
            return await Get<EmployeeModel>($"/api/employee/{id}").ConfigureAwait(true);
        }

        public async Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        {
            return await Get<List<EmployeeModel>>($"/api/employee").ConfigureAwait(true);
        }

        public Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}
