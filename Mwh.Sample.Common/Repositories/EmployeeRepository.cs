using Mwh.Sample.Common.Interfaces;
using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Class EmployeeRepository.
    /// Implements the <see cref="Mwh.Sample.Common.Repositories.IEmployeeRepository" />
    /// </summary>
    /// <seealso cref="Mwh.Sample.Common.Repositories.IEmployeeRepository" />
    public class EmployeeRepository : IEmployeeRepository
    {
        /// <summary>
        /// The emp
        /// </summary>
        private readonly IEmployeeDB _emp;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeRepository"/> class.
        /// </summary>
        /// <param name="employeeDB">The employee database.</param>
        public EmployeeRepository(IEmployeeDB employeeDB) { _emp = employeeDB; }

        /// <summary>
        /// add as an asynchronous operation.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<EmployeeModel> AddAsync(EmployeeModel employee, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return await Task.Run(() => _emp.Update(employee)).ConfigureAwait(true);

        }

        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;EmployeeModel&gt;.</returns>
        public Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => _emp.Employee(id));
        }

        /// <summary>
        /// Lists the asynchronous.
        /// </summary>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IEnumerable&lt;EmployeeModel&gt;&gt;.</returns>
        public Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => (IEnumerable<EmployeeModel>)_emp.EmployeeCollection());
        }

        /// <summary>
        /// Removes the specified employee.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public Task<bool> Remove(EmployeeModel employee, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => (bool)_emp.Delete(employee.id));
        }

        /// <summary>
        /// Updates the specified employee.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;EmployeeModel&gt;.</returns>
        public Task<EmployeeModel> Update(EmployeeModel employee, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => (EmployeeModel)_emp.Update(employee));
        }
    }
}
