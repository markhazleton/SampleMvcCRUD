using Mwh.Sample.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mwh.Sample.Common.Interfaces
{
    /// <summary>
    /// Interface IEmployeeRepository
    /// </summary>
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Lists the asynchronous.
        /// </summary>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;IEnumerable&lt;EmployeeModel&gt;&gt;.</returns>
        Task<IEnumerable<EmployeeModel>> ListAsync(CancellationToken token);
        /// <summary>
        /// Adds the asynchronous.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task<EmployeeModel> AddAsync(EmployeeModel employee, CancellationToken token);
        /// <summary>
        /// Finds the by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;EmployeeModel&gt;.</returns>
        Task<EmployeeModel> FindByIdAsync(int id, CancellationToken token);
        /// <summary>
        /// Updates the specified employee.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;EmployeeModel&gt;.</returns>
        Task<EmployeeModel> Update(EmployeeModel employee, CancellationToken token);
        /// <summary>
        /// Removes the specified employee.
        /// </summary>
        /// <param name="employee">The employee.</param>
        /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        Task<bool> Remove(EmployeeModel employee, CancellationToken token);
    }
}
