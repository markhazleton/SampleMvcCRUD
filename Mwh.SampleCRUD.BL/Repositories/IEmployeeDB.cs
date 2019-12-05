using Mwh.SampleCRUD.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.SampleCRUD.BL.Repositories
{
    /// <summary>
    /// Employee DAL Interface
    /// </summary>
    public interface IEmployeeDB
    {
        int Delete(int ID);

        EmployeeModel Get(int id);

        List<EmployeeModel> ListAll();

        int Update(EmployeeModel emp);
    }
}
