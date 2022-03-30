
namespace Mwh.Sample.Domain.Interfaces;
/// <summary>
/// Interface IEmployeeRepository
/// </summary>
public interface IEmployeeRepository
{

    /// <summary>
    /// Add Employee.
    /// </summary>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task.</returns>
    Task<EmployeeDto> AddAsync(EmployeeDto employee, CancellationToken token);
    /// <summary>
    /// Add Department
    /// </summary>
    /// <param name="department"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<DepartmentDto> AddAsync(DepartmentDto department, CancellationToken token);

    /// <summary>
    /// Finds the by identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;EmployeeModel&gt;.</returns>
    Task<EmployeeDto> FindEmployeeByIdAsync(int id, CancellationToken token);
    /// <summary>
    /// 
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
    Task<IEnumerable<EmployeeDto>> ListEmployeesAsync(CancellationToken token);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<IEnumerable<DepartmentDto>> ListDepartmentsAsync(CancellationToken token);

    /// <summary>
    /// Removes the specified employee.
    /// </summary>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;System.Boolean&gt;.</returns>
    Task<bool> RemoveAsync(EmployeeDto employee, CancellationToken token);

    /// <summary>
    /// Updates the specified employee.
    /// </summary>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>Task&lt;EmployeeModel&gt;.</returns>
    Task<EmployeeDto> UpdateAsync(EmployeeDto employee, CancellationToken token);
    /// <summary>
    /// Update the Department
    /// </summary>
    /// <param name="department"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<DepartmentDto> UpdateAsync(DepartmentDto department, CancellationToken token);
}
