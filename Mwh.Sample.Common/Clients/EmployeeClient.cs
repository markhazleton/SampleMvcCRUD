﻿using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Clients
{
    /// <summary>
    /// Class SampleClient.
    /// Implements the <see cref="Mwh.Sample.Client.ClientBase" />
    /// Implements the <see cref="Mwh.Sample.Common.Repositories.IEmployeeService" />
    /// </summary>
    /// <seealso cref="Mwh.Sample.Client.ClientBase" />
    /// <seealso cref="Mwh.Sample.Common.Repositories.IEmployeeService" />
    public class EmployeeClient : ClientBase, IEmployeeService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeClient"/> class.
        /// </summary>
        /// <param name="apiPath">The API path.</param>
        /// <param name="appName">Name of the application.</param>
        public EmployeeClient(string apiPath, string appName) : base(apiPath, appName)
        {

        }
        /// <summary>
        /// delete as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var resp = await Delete<EmployeeResponse>($"/api/employee/{id}").ConfigureAwait(true);
            return resp == null ? new EmployeeResponse("Null Returned from REST Call to Delete") : resp;
        }

        /// <summary>
        /// find by identifier as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeModel.</returns>
        public async Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return await GetAsync<EmployeeModel>($"/api/employee/{id}").ConfigureAwait(true);
        }

        /// <summary>
        /// list as an asynchronous operation.
        /// </summary>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>IEnumerable&lt;EmployeeModel&gt;.</returns>
        public async Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return await GetAsync<List<EmployeeModel>>($"/api/employee").ConfigureAwait(true);
        }

        /// <summary>
        /// save as an asynchronous operation.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public async Task<EmployeeResponse> SaveAsync(EmployeeModel employee, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var resp = await Post<EmployeeResponse>($"/api/employee", employee).ConfigureAwait(true);
            return resp;
        }

        /// <summary>
        /// update as an asynchronous operation.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>EmployeeResponse.</returns>
        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeModel employee, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (employee.id != id)
                return new EmployeeResponse($"Mismatch in id({id}) && id({employee.id}).");

            var resp = await Put<EmployeeResponse>($"/api/employee/{id}", employee).ConfigureAwait(true);
            return resp;
        }
    }
}
