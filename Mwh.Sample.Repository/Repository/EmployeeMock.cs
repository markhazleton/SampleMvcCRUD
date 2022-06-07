
using Bogus;

namespace Mwh.Sample.Repository.Repository;
/// <summary>
/// Employee Mock Repository
/// </summary>
public class EmployeeMock : IEmployeeDB
{
    private List<DepartmentDto> _depts;
    /// <summary>
    /// The list
    /// </summary>
    private List<EmployeeDto> _emps;




    /// <summary>
    /// Constructor
    /// </summary>
    public EmployeeMock()
    {
        _depts = new List<DepartmentDto>();

        foreach (var dept in Enum.GetValues(typeof(EmployeeDepartmentEnum)))
        {
            _depts.Add(new DepartmentDto()
            {
                Id = (int)dept,
                Name = dept?.ToString() ?? "UNKNOWN",
                Description = dept?.ToString() ?? "UNKNOWN",
            });
        }
        _emps = new List<EmployeeDto>()
            {
            new EmployeeDto() { Name = "Ilsa Lund", Age = 25, Country = "Germany", Department = EmployeeDepartmentEnum.IT, State = "Hesse" },
            new EmployeeDto() { Name = "Major Strasser", Age = 35, Country = "Germany", Department = EmployeeDepartmentEnum.IT, State = "Hesse" },
            new EmployeeDto() { Name = "Rick Blaine", Age = 45, Country = "USA", Department = EmployeeDepartmentEnum.IT, State = "New York" },
            new EmployeeDto() { Name = "Victor Laszlo", Age = 55, Country = "Germany", Department = EmployeeDepartmentEnum.IT, State = "Hesse" },
            new EmployeeDto() { Name = "Louis Renault", Age = 65, Country = "France", Department = EmployeeDepartmentEnum.IT, State = "Brittany" },
            new EmployeeDto() { Name = "Sam Spade", Age = 55, Country = "USA", Department = EmployeeDepartmentEnum.IT, State = "California" },
            new EmployeeDto() { Name = "Jim Smith",Age = 35,Department = EmployeeDepartmentEnum.IT,State = "Florida",Country = "USA"},
            new EmployeeDto() { Name = "Bob Roberts",Age = 50,Department = EmployeeDepartmentEnum.HR,State = "Texas",Country = "USA"},
            new EmployeeDto() { Name = "Sam Malone",Age = 53,Department = EmployeeDepartmentEnum.Marketing,State = "Massachusetts",Country = "USA"},
            new EmployeeDto() { Name = "Frank Sinatra",Age = 50,Department = EmployeeDepartmentEnum.Executive,State = "New York",Country = "USA"},
            };

        GetEmployeeList(300).ForEach(e =>
        {
            var emp = Create(e);
            if (emp is not null) _emps.Add(emp);
        });

        for (int i = 0; i < _emps.Count; i++)
        {
            _emps[i].Id = i + 1;
        }
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
    public async Task<DepartmentDto> DepartmentAsync(int id)
    {
        DepartmentDto? department = null;
        await Task.Run(() =>
        {
            department = _depts?.Where(w => w.Id == id).FirstOrDefault() ?? new DepartmentDto();
        });
        return department ?? new DepartmentDto();
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
    public async Task<EmployeeDto> EmployeeAsync(int id)
    {
        var emp = new EmployeeDto();
        await Task.Run(() =>
        {
            emp = _emps?.Where(w => w.Id == id).FirstOrDefault() ?? new EmployeeDto();
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
    //Method for Updating Employee record
    /// <summary>
    /// Updates the specified emp.
    /// </summary>
    /// <param name="emp">The emp.</param>
    /// <returns>EmployeeModel.</returns>
    public async Task<EmployeeDto> UpdateAsync(EmployeeDto emp)
    {
        if (emp == null)
            return new EmployeeDto();

        if (!emp.IsValid)
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
            var updateEmp = _emps.Where(w => w.Id == emp.Id).FirstOrDefault();

            if (updateEmp == null)
                return new EmployeeDto();

            updateEmp.Name = emp.Name;
            updateEmp.Age = emp.Age;
            updateEmp.Department = emp.Department;
            updateEmp.Country = emp.Country;
            updateEmp.State = emp.State;
            return updateEmp;
        }
    }

    public async Task<DepartmentDto> UpdateAsync(DepartmentDto dept)
    {
        return dept;
    }

    public static EmployeeDto? Create(Employee? item)
    {
        if (item == null) return null;

        return new EmployeeDto()
        {
            Name = item.Name,
            State = item?.State ?? String.Empty,
            Country = item?.Country ?? string.Empty,
            Age = item?.Age ?? 0,
            Department = (EmployeeDepartmentEnum)(item?.DepartmentId ?? 1),
            DepartmentName = ((EmployeeDepartmentEnum)(item?.DepartmentId ?? 0)).ToString() ?? string.Empty,
            Id = item?.Id ?? 0
        };
    }

    public static List<Employee> GetEmployeeList(int generateCount)
    {
        var states = new string[] { "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming" };
        var testUsers = new Faker<Employee>()
           //Optional: Call for objects that have complex initialization
           .CustomInstantiator(f => new Employee())
           //Basic rules using built-in generators
           .RuleFor(u => u.Name, (f, u) => f.Name.FullName())
           .RuleFor(u => u.Age, f => f.Random.Number(18, 70))
           .RuleFor(u => u.DepartmentId, f => f.Random.Number(1, 6))
           .RuleFor(u => u.Country, "USA")
           .RuleFor(u => u.State, f => f.Random.ListItem(states))
           //After all rules are applied finish with the following action
           .FinishWith((f, u) =>
           {
               Console.WriteLine($"Employee Created! Name={u.Name}");
           });
        var user = testUsers.Generate(generateCount);
        return user;
    }



}
