
namespace Mwh.Sample.Domain.Models;
public class EmployeeDto
{
    /// <summary>
    /// 
    /// </summary>
    public EmployeeDto()
    {

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Name"></param>
    /// <param name="dept"></param>
    public EmployeeDto(int id, string name, int age, string state, string country, EmployeeDepartmentEnum dept)
    {
        Id = id;
        Name = name;
        Age = age;
        State = state;
        Country = country;
        Department = dept;
        if (!IsValid())
            throw new ArgumentException("New Employee is not valid", nameof(name));

    }

    /// <summary>
    /// Returns true if ... is valid.
    /// </summary>
    /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Name))
            return false;
        if (string.IsNullOrEmpty(State))
            return false;
        if (string.IsNullOrEmpty(Country))
            return false;
        if (Department == EmployeeDepartmentEnum.Unknown)
            return false;
        if ((Age < 1))
            return false;
        return true;
    }

    /// <summary>
    /// Gets or sets the age.
    /// </summary>
    /// <value>The age.</value>
    [JsonPropertyName("age")]
    public int Age { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    /// <value>The country.</value>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    /// <value>The department.</value>
    [JsonPropertyName("department")]
    public EmployeeDepartmentEnum Department { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("departmentName")]
    public string? DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    /// <value>The employee identifier.</value>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [Required]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    /// <value>The state.</value>
    [Required]
    [JsonPropertyName("state")]
    public string? State { get; set; }


    /// <summary>
    /// Employee Manager
    /// </summary>
    [JsonPropertyName("manager_id")]
    public int? ManagerId { get; set; }
}
