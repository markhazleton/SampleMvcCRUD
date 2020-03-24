using Mwh.SampleCRUD.BL.Models;
using System.Collections.Generic;

namespace Mwh.SampleCRUD.BL.Repositories
{
    /// <summary>
    /// Employee DAL Interface
    /// </summary>
    public interface IEmployeeDB
    {
        int Delete(int ID);

        EmployeeModel Employee(int id);

        List<EmployeeModel> EmployeeCollection();

        int Update(EmployeeModel emp);
    }
}
