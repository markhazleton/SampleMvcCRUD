
namespace Mwh.Sample.Domain.Interfaces;
/// <summary>
/// Employee Database  Interface
/// </summary>
public interface IEmployeeDB
{
    /// <summary>
    /// Deletes the specified identifier.
    /// </summary>
    /// <param name="ID">The identifier.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    Task<bool> DeleteEmployeeAsync(int ID);

    /// <summary>
    /// Employees the specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>EmployeeModel.</returns>
    Task<EmployeeDto> EmployeeAsync(int id);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<DepartmentDto> DepartmentAsync(int id);

    /// <summary>
    /// Employees the collection.
    /// </summary>
    /// <returns>List&lt;EmployeeModel&gt;.</returns>
    Task<List<EmployeeDto>> EmployeeCollectionAsync();
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<List<DepartmentDto>> DepartmentCollectionAsync();

    /// <summary>
    /// Updates the specified emp.
    /// </summary>
    /// <param name="employee">The emp.</param>
    /// <returns>EmployeeModel.</returns>
    Task<EmployeeDto> UpdateAsync(EmployeeDto employee);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="department"></param>
    /// <returns></returns>
    Task<DepartmentDto> UpdateAsync(DepartmentDto department);
}
