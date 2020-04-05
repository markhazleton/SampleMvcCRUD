using Mwh.Sample.Common.Models;
using System.Collections.Generic;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Employee DAL Interface
    /// </summary>
    public interface IEmployeeDB
    {
        bool Delete(int ID);

        EmployeeModel Employee(int id);

        List<EmployeeModel> EmployeeCollection();

        EmployeeModel Update(EmployeeModel emp);
    }
}
