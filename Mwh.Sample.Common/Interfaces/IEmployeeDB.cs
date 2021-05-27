using Mwh.Sample.Common.Models;
using System.Collections.Generic;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Employee DAL Interface
    /// </summary>
    public interface IEmployeeDB
    {
        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="ID">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool Delete(int ID);

        /// <summary>
        /// Employees the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>EmployeeModel.</returns>
        EmployeeModel Employee(int id);

        /// <summary>
        /// Employees the collection.
        /// </summary>
        /// <returns>List&lt;EmployeeModel&gt;.</returns>
        List<EmployeeModel> EmployeeCollection();

        /// <summary>
        /// Updates the specified emp.
        /// </summary>
        /// <param name="emp">The emp.</param>
        /// <returns>EmployeeModel.</returns>
        EmployeeModel Update(EmployeeModel emp);
    }
}
