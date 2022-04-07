﻿
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
            new EmployeeDto() { Name = "Ilsa Lund", Age = 25, Country = "Germany", Department = EmployeeDepartmentEnum.IT, State = "TX" },
            new EmployeeDto() { Name = "Major Strasser", Age = 35, Country = "Germany", Department = EmployeeDepartmentEnum.IT, State = "TX" },
            new EmployeeDto() { Name = "Rick Blaine", Age = 45, Country = "USA", Department = EmployeeDepartmentEnum.IT, State = "TX" },
            new EmployeeDto() { Name = "Victor Laszlo", Age = 55, Country = "Germany", Department = EmployeeDepartmentEnum.IT, State = "TX" },
            new EmployeeDto() { Name = "Louis Renault", Age = 65, Country = "France", Department = EmployeeDepartmentEnum.IT, State = "TX" },
            new EmployeeDto() { Name = "Sam Spade", Age = 55, Country = "USA", Department = EmployeeDepartmentEnum.IT, State = "TX" },
            new EmployeeDto() { Name = "Jim",Age = 35,Department = EmployeeDepartmentEnum.IT,State = "Florida",Country = "USA"},
            new EmployeeDto() { Name = "Bob",Age = 50,Department = EmployeeDepartmentEnum.HR,State = "Texas",Country = "USA"},
            new EmployeeDto() { Name = "Sam",Age = 53,Department = EmployeeDepartmentEnum.Marketing,State = "Texas",Country = "USA"},
            new EmployeeDto() { Name = "Frank",Age = 50,Department = EmployeeDepartmentEnum.Executive,State = "Texas",Country = "USA"},
            };

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
        var myEmp = _emps.Where(w => w.Id == ID).FirstOrDefault();
        if (myEmp == null)
            return false;

        return _emps.Remove(myEmp);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<DepartmentDto> DepartmentAsync(int id)
    {
        return _depts?.Where(w => w.Id == id).FirstOrDefault() ?? new DepartmentDto();

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
    public async Task<List<DepartmentDto>> DepartmentCollectionAsync() { return _depts; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<EmployeeDto> EmployeeAsync(int id)
    {
        return _emps?.Where(w => w.Id == id).FirstOrDefault() ?? new EmployeeDto();
    }

    //Return list of all Employees
    /// <summary>
    /// Employees the collection.
    /// </summary>
    /// <returns>List&lt;EmployeeModel&gt;.</returns>
    public List<EmployeeDto> EmployeeCollection() { return _emps; }

    public async Task<List<EmployeeDto>> EmployeeCollectionAsync()
    {
        return _emps;
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
            int nextID = _emps.OrderByDescending(o => o.Id).Select(s => s.Id).FirstOrDefault() + 1;
            emp.Id = nextID;
            _emps.Add(emp);
            return emp;
        }
        else
        {
            var myEmp = _emps.Where(w => w.Id == emp.Id).FirstOrDefault();

            if (myEmp == null)
                return new EmployeeDto();

            myEmp.Name = emp.Name;
            myEmp.Age = emp.Age;
            myEmp.Department = emp.Department;
            myEmp.Country = emp.Country;
            myEmp.State = emp.State;
            return myEmp;
        }
    }

    public async Task<DepartmentDto> UpdateAsync(DepartmentDto dept)
    {
        return dept;
    }
}
