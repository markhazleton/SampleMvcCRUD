using Microsoft.AspNetCore.Http;
using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.HttpClientFactory.Clients
{
    /// <summary>
    ///
    /// </summary>
    public class EmployeeRestClient : RestClientBase, IEmployeeClient
    {
        public EmployeeRestClient(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory) : base($"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}", "employee", httpClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeRestClient"/> class.
        /// </summary>
        /// <param name="apiPath">The API path.</param>
        /// <param name="appName">Name of the application.</param>
        public EmployeeRestClient(string apiPath, string appName, IHttpClientFactory httpClientFactory) : base(apiPath, appName, httpClientFactory)
        {
        }

        /// <summary>
        /// delete as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return DeleteAsync<EmployeeResponse>($"/api/employee/{id}",token);
        }

        /// <summary>
        /// find by identifier as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeModel.</returns>
        public Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return GetAsync<EmployeeModel>($"/api/employee/{id}",token);
        }

        /// <summary>
        /// list as an asynchronous operation.
        /// </summary>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>IEnumerable&lt;EmployeeModel&gt;.</returns>
        public Task<IEnumerable<EmployeeModel>> GetAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return GetAsync<IEnumerable<EmployeeModel>>($"/api/employee",token);
        }

        /// <summary>
        /// save as an asynchronous operation.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return PostAsync<EmployeeResponse>($"/api/employee", employee,token);
        }

        /// <summary>
        /// update as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public Task<EmployeeResponse> UpdateAsync(int id, 
            EmployeeModel employee, 
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (employee.id == id)
            {
                return PutAsync<EmployeeResponse>($"/api/employee/{id}", employee,token);
            }
            return Task.Run(() => { 
                return new EmployeeResponse($"Mismatch in id({id}) && id({employee.id})."); 
            });
        }
    }
}