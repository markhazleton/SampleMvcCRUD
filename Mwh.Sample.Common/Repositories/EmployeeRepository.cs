
namespace Mwh.Sample.Common.Repositories;

/// <summary>
/// Class EmployeeRepository.
/// Implements the <see cref="Repositories.IEmployeeRepository" />
/// </summary>
/// <seealso cref="Repositories.IEmployeeRepository" />
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
    public Task<EmployeeModel> AddAsync(EmployeeModel employee, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.Run(() => _emp.Update(employee));
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
    public Task<bool> RemoveAsync(EmployeeModel employee, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.Run(() => _emp.Delete(employee.id));
    }

    /// <summary>
    /// Updates the specified employee.
    /// </summary>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;EmployeeModel&gt;.</returns>
    public Task<EmployeeModel> UpdateAsync(EmployeeModel employee, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return Task.Run(() => _emp.Update(employee));
    }
}
