
namespace Mwh.Sample.Common.Repositories;

/// <summary>
/// Employee Mock Repository
/// </summary>
public class EmployeeMock : IEmployeeDB
{
    /// <summary>
    /// The list
    /// </summary>
    private List<EmployeeModel> _list;

    /// <summary>
    /// Constructor
    /// </summary>
    public EmployeeMock()
    {
        _list = new List<EmployeeModel>()
            {
                new EmployeeModel()
                {
                    Name = "Jim",
                    Age = 35,
                    Department = EmployeeDepartment.IT,
                    State = "Florida",
                    Country = "USA",
                    id = 4
                },
                new EmployeeModel()
                {
                    Name = "Bob",
                    Age = 50,
                    Department = EmployeeDepartment.IT,
                    State = "Texas",
                    Country = "USA",
                    id = 1
                },
                new EmployeeModel()
                {
                    Name = "Sam",
                    Age = 53,
                    Department = EmployeeDepartment.Marketing,
                    State = "Texas",
                    Country = "USA",
                    id = 2
                },
                new EmployeeModel()
                {
                    Name = "Frank",
                    Age = 50,
                    Department = EmployeeDepartment.Executive,
                    State = "Texas",
                    Country = "USA",
                    id = 3
                },
            };
    }

    /// <summary>
    /// Method for Deleting an Employee
    /// </summary>
    /// <param name="ID">The identifier.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public bool Delete(int ID)
    {
        var myEmp = _list.Where(w => w.id == ID).FirstOrDefault();
        if (myEmp == null)
            return false;
        if (_list.Remove(myEmp))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Employees the specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>EmployeeModel.</returns>
    public EmployeeModel Employee(int id)
    {
        var myEmp = _list.Where(w => w.id == id).FirstOrDefault();
        if (myEmp == null)
            return new EmployeeModel();
        return myEmp;
    }

    //Return list of all Employees
    /// <summary>
    /// Employees the collection.
    /// </summary>
    /// <returns>List&lt;EmployeeModel&gt;.</returns>
    public List<EmployeeModel> EmployeeCollection() { return _list; }

    //Method for Updating Employee record
    /// <summary>
    /// Updates the specified emp.
    /// </summary>
    /// <param name="emp">The emp.</param>
    /// <returns>EmployeeModel.</returns>
    public EmployeeModel Update(EmployeeModel emp)
    {
        if (emp == null)
            return new EmployeeModel();

        if (!emp.IsValid)
            return emp;

        if (emp.id == 0)
        {
            int nextID = _list.OrderByDescending(o => o.id).Select(s => s.id).FirstOrDefault() + 1;
            emp.id = nextID;
            _list.Add(emp);
            return emp;
        }
        else
        {
            var myEmp = _list.Where(w => w.id == emp.id).FirstOrDefault();

            if (myEmp == null)
                return new EmployeeModel();

            myEmp.Name = emp.Name;
            myEmp.Age = emp.Age;
            myEmp.Department = emp.Department;
            myEmp.Country = emp.Country;
            myEmp.State = emp.State;
            return myEmp;
        }
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static EmployeeMock Instance { get; } = new EmployeeMock();
}
