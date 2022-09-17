using Bogus;

namespace Mwh.Sample.Repository.Repository;
/// <summary>
/// Employee Mock Repository
/// </summary>
public class EmployeeMock : IEmployeeDB
{
    private readonly List<DepartmentDto> _depts;
    private readonly List<EmployeeDto> _emps;
    private readonly int _generatedEmployeeCount = 0;

    /// <summary>
    /// Constructor
    /// </summary>
    public EmployeeMock(int GeneratedEmployeeCount = 0)
    {
        _generatedEmployeeCount = GeneratedEmployeeCount;
        _depts = new List<DepartmentDto>();
        foreach (var dept in Enum.GetValues(typeof(EmployeeDepartmentEnum)))
        {
            if ((int)dept > 0)
            {
                _depts.Add(new DepartmentDto((int)dept, dept?.ToString() ?? "UNKNOWN", dept?.ToString() ?? "UNKNOWN"));
            }
        }
        var FixedEmployees = new List<Employee>()
            {
            new Employee() { Name = "Ilsa Lund", Age = 25, Country = "USA", DepartmentId = (int)EmployeeDepartmentEnum.IT, State = "Kansas" },
            new Employee() { Name = "Major Strasser", Age = 35, Country = "USA", DepartmentId = (int)EmployeeDepartmentEnum.IT, State = "Texas" },
            new Employee() { Name = "Rick Blaine", Age = 45, Country = "USA", DepartmentId = (int)EmployeeDepartmentEnum.IT, State = "New York" },
            new Employee() { Name = "Victor Laszlo", Age = 55, Country = "USA", DepartmentId = (int)EmployeeDepartmentEnum.IT, State = "Colorado" },
            new Employee() { Name = "Louis Renault", Age = 65, Country = "USA", DepartmentId = (int)EmployeeDepartmentEnum.IT, State = "Idaho" },
            new Employee() { Name = "Sam Spade", Age = 55, Country = "USA", DepartmentId = (int)EmployeeDepartmentEnum.IT, State = "California" },
            new Employee() { Name = "Jim Smith",Age = 35,DepartmentId = (int)EmployeeDepartmentEnum.IT,State = "Florida",Country = "USA"},
            new Employee() { Name = "Bob Roberts",Age = 50,DepartmentId = (int)EmployeeDepartmentEnum.HR,State = "Texas",Country = "USA"},
            new Employee() { Name = "Sam Malone",Age = 53,DepartmentId = (int)EmployeeDepartmentEnum.Marketing,State = "Massachusetts",Country = "USA"},
            new Employee() { Name = "Frank Sinatra",Age = 50,DepartmentId = (int)EmployeeDepartmentEnum.Executive,State = "New York",Country = "USA"},
            };
        FixedEmployees.AddRange(GetEmployeeList(_generatedEmployeeCount));

        _emps = new List<EmployeeDto>();
        for (int i = 1; i < FixedEmployees.Count; i++)
        {
            try
            {
                var emp = Create(FixedEmployees[i], i);
                if (emp is not null) _emps.Add(emp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private static EmployeeDto? Create(Employee? item, int id)
    {
        if (item == null) return null;

        EmployeeDepartmentEnum empDept = (EmployeeDepartmentEnum)(item?.DepartmentId ?? 1);
        return new EmployeeDto(
            id,
            item?.Name ?? string.Empty,
            item?.Age ?? 99,
            item?.State ?? string.Empty,
            item?.Country ?? string.Empty,
            empDept
        );
    }

    /// <summary>
    /// Method for Deleting an Employee
    /// </summary>
    /// <param name="ID">The identifier.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public async Task<bool> DeleteEmployeeAsync(int ID)
    {
        bool bReturn = false;
        await Task.Run(() =>
        {
            var myEmp = _emps.Where(w => w.Id == ID).FirstOrDefault();
            if (myEmp != null)
                bReturn = _emps.Remove(myEmp);
        });
        return bReturn;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<DepartmentDto?> DepartmentAsync(int id)
    {
        DepartmentDto? department = null;
        await Task.Run(() =>
        {
            department = _depts?.Where(w => w.Id == id).FirstOrDefault();
        });
        return department;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<DepartmentDto> DepartmentCollection() { return _depts; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<List<DepartmentDto>> DepartmentCollectionAsync()
    {

        var list = new List<DepartmentDto>();
        await Task.Run(() =>
        {
            list.AddRange(_depts);
        });
        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<EmployeeDto?> EmployeeAsync(int id)
    {
        EmployeeDto? emp = null;
        await Task.Run(() =>
        {
            emp = _emps?.Where(w => w.Id == id).FirstOrDefault() ?? null;
        });
        return emp;
    }

    //Return list of all Employees
    /// <summary>
    /// Employees the collection.
    /// </summary>
    /// <returns>List&lt;EmployeeModel&gt;.</returns>
    public List<EmployeeDto> EmployeeCollection() { return _emps; }

    public async Task<List<EmployeeDto>> EmployeeCollectionAsync()
    {
        var list = new List<EmployeeDto>();
        await Task.Run(() =>
        {
            list.AddRange(_emps);
        });
        return list;
    }

    public static List<Employee> GetEmployeeList(int generateCount)
    {
        var states = new string[] { "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming" };
        var fakeEmployees = new Faker<Employee>()
           //Optional: Call for objects that have complex initialization
           .CustomInstantiator(f => new Employee())
           //Basic rules using built-in generators
           .RuleFor(u => u.Name, (f, u) => f.Name.FullName())
           .RuleFor(u => u.Age, f => f.Random.Number(18, 70))
           .RuleFor(u => u.DepartmentId, f => f.Random.Number(1, 6))
           .RuleFor(u => u.Country, "USA")
           .RuleFor(u => u.State, f => f.Random.ListItem(states))
           //After all rules are applied finish with the following action
           .FinishWith((f, u) => { });
        return fakeEmployees.Generate(generateCount);
    }
    //Method for Updating Employee record
    /// <summary>
    /// Updates the specified emp.
    /// </summary>
    /// <param name="emp">The emp.</param>
    /// <returns>EmployeeModel.</returns>
    public async Task<EmployeeDto?> UpdateAsync(EmployeeDto? emp)
    {
        if (emp == null)
            return null;

        if (!emp.IsValid())
            return emp;

        if (emp.Id == 0)
        {
            await Task.Run(() =>
            {
                int nextID = _emps.OrderByDescending(o => o.Id).Select(s => s.Id).FirstOrDefault() + 1;
                emp.Id = nextID;
                _emps.Add(emp);
            });
            return emp;
        }
        else
        {
            var updateEmp = _emps.Find(o => o.Id == emp.Id);
            if (updateEmp == null)
            {
                _emps.Add(emp);
            }
            else
            {
                updateEmp.Name = emp.Name;
                updateEmp.Age = emp.Age;
                updateEmp.Department = emp.Department;
                updateEmp.Country = emp.Country;
                updateEmp.State = emp.State;
            }
            return emp;
        }
    }

    /// <summary>
    /// Update Department 
    /// </summary>
    /// <param name="dept"></param>
    /// <returns></returns>
    public async Task<DepartmentDto?> UpdateAsync(DepartmentDto? dept)
    {
        await Task.Run(() =>
        {
            // TODO: Update Department
        });
        return dept;
    }



}
