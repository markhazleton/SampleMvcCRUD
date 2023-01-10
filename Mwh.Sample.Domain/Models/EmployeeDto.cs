
namespace Mwh.Sample.Domain.Models;
public class EmployeeDto : IComparable<EmployeeDto>
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

    public static bool operator !=(EmployeeDto left, EmployeeDto right)
    {
        return !(left == right);
    }

    public static bool operator <(EmployeeDto left, EmployeeDto right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(EmployeeDto left, EmployeeDto right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator ==(EmployeeDto left, EmployeeDto right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator >(EmployeeDto left, EmployeeDto right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(EmployeeDto left, EmployeeDto right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }

    public int CompareTo(EmployeeDto? other)
    {
        if (other == null) return 1;
        // Get a list of all public properties of the Person class
        var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Compare the values of the public properties
        foreach (var property in properties)
        {
            var value1 = property.GetValue(this);
            var value2 = property.GetValue(other);

            if (value1 == null && value2 == null)
            {
                // Both objects are null, so they are equal
                return 0;
            }
            else if (value1 == null || value2 == null)
            {
                // Only one object is null, so they are not equal
                return 1;
            }
            else
            {
                // If the values are not equal, return the result of calling CompareTo on the values
                if (!value1.Equals(value2))
                {
                    return ((IComparable)value1).CompareTo(value2);
                }
            }
        }
        // If all public property values are equal, return 0
        return 0;
    }

    public override bool Equals(object obj)
    {
        var other = obj as EmployeeDto;
        if (other == null) return false;

        // Get a list of all public properties of the Person class
        var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Compare the values of the public properties
        foreach (var property in properties)
        {
            var value1 = property.GetValue(this);
            var value2 = property.GetValue(other);

            // If either value is null, use the ReferenceEquals method to compare them
            if (value1 == null || value2 == null)
            {
                if (!Object.ReferenceEquals(value1, value2))
                {
                    return false;
                }
            }
            else
            {
                // If both values are not null, use the Equals method to compare them
                if (!value1.Equals(value2))
                {
                    return false;
                }
            }
        }
        // If all public property values are equal, return true
        return true;
    }

    public override int GetHashCode()
    {
        // Serialize the object to a string
        var serialized = JsonSerializer.Serialize(this);
        // Get the hash code of the serialized string
        return serialized.GetHashCode();
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
    /// Employee Manager
    /// </summary>
    [JsonPropertyName("manager_id")]
    public int? ManagerId { get; set; }

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
}
