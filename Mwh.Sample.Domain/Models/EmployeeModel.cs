
namespace Mwh.Sample.Domain.Models;
/// <summary>
/// Employee Model
/// </summary>
public class EmployeeModel
{
    /// <summary>
    /// Constructor
    /// </summary>
    public EmployeeModel()
    {
        Department = EmployeeDepartment.Unknown;
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
    public string Country { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    /// <value>The department.</value>
    [JsonPropertyName("department")]
    public EmployeeDepartment Department { get; set; }

    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    /// <value>The employee identifier.</value>
    [JsonPropertyName("id")]
    public int id { get; set; }

    /// <summary>
    /// Returns true if ... is valid.
    /// </summary>
    /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
    public bool IsValid
    {
        get
        {
            if (string.IsNullOrEmpty(Name))
                return false;
            if (string.IsNullOrEmpty(State))
                return false;
            if (string.IsNullOrEmpty(Country))
                return false;
            if (!Department.IsDefined())
                return false;
            if ((Age < 1))
                return false;
            return true;
        }
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    /// <value>The state.</value>
    [Required]
    [JsonPropertyName("state")]
    public string State { get; set; }
}
