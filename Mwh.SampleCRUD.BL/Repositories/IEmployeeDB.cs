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

        Employee Get(int id);

        List<Employee> ListAll();

        int Update(Employee emp);
    }
}
