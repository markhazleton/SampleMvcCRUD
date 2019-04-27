namespace SampleCRUD.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IEmployeeDB
    {
        int Delete(int ID);

        Employee Get(int id);

        List<Employee> ListAll();

        int Update(Employee emp);
    }
}
