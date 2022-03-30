
namespace Mwh.Sample.Domain.Interfaces;
/// <summary>
///
/// </summary>
public interface IEmployeeClient  
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="namelist"></param>
    /// <returns></returns>
    Task<int> AddMultipleEmployeesAsync(string[]? namelist);
    /// <summary>
    /// Deletes the asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;EmployeeResponse&gt;.</returns>
    Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token);

    /// <summary>
    /// Finds the by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;EmployeeModel&gt;.</returns>
    Task<EmployeeDto> FindEmployeeByIdAsync(int id, CancellationToken token);
    /// <summary>
    /// Find the Department by the id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<DepartmentDto> FindDepartmentByIdAsync(int id, CancellationToken token);

    /// <summary>
    /// Lists the asynchronous.
    /// </summary>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;IEnumerable&lt;EmployeeModel&gt;&gt;.</returns>
    Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(CancellationToken token);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken token);

    /// <summary>
    /// Saves the asynchronous.
    /// </summary>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;EmployeeResponse&gt;.</returns>
    Task<EmployeeResponse> SaveAsync(EmployeeDto employee, CancellationToken token);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dept"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<DepartmentResponse> SaveAsync(DepartmentDto dept, CancellationToken token);
    /// <summary>
    /// Updates the asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;EmployeeResponse&gt;.</returns>
    Task<EmployeeResponse> UpdateAsync(int id, EmployeeDto employee, CancellationToken token);
}
