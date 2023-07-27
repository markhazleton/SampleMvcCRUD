using Microsoft.AspNetCore.Http;

namespace Mwh.Sample.Domain.Models;



public class EmployeeDto : IComparable<EmployeeDto>, IEmployeeDto
{
    /// <summary>
    /// Employee DTO
    /// </summary>
    public EmployeeDto()
    {

    }

    /// <summary>
    /// Load the Employee Dto
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="age"></param>
    /// <param name="state"></param>
    /// <param name="country"></param>
    /// <param name="dept"></param>
    /// <param name="profilePicture"></param>
    /// <exception cref="ArgumentException"></exception>
    public EmployeeDto(int id,
        string name,
        int age,
        string state,
        string country,
        EmployeeDepartmentEnum dept,
        string? profilePicture = null,
        GenderEnum? gender = null)
    {
        Id = id;
        Name = name;
        Age = age;
        State = state;
        Country = country;
        Department = dept;
        ProfilePicture = profilePicture;
        Gender = gender ?? GenderEnum.Other;

        EnsureValidDetails();
    }

    public static bool operator !=(EmployeeDto left, EmployeeDto right)
    {
        return !(left == right);
    }

    public static bool operator <(EmployeeDto left, EmployeeDto right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(EmployeeDto left, EmployeeDto right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator ==(EmployeeDto left, EmployeeDto right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator >(EmployeeDto left, EmployeeDto right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(EmployeeDto left, EmployeeDto right)
    {
        return ReferenceEquals(left, null) ? right is null : left.CompareTo(right) >= 0;
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
                // Both properties are null, so they are equal
                continue;
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

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not EmployeeDto other)
            return false;

        // Get a list of all public properties of the EmployeeDto class
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Compare the values of the public properties
        foreach (var property in properties)
        {
            var value1 = property.GetValue(this);
            var value2 = property.GetValue(other);

            if (!AreEqual(value1, value2))
                return false;
        }

        return true;
    }

    private static bool AreEqual(object? value1, object? value2)
    {
        if (ReferenceEquals(value1, value2))
            return true;

        return value1?.Equals(value2) ?? false;
    }

    public override int GetHashCode()
    {
        // Serialize the object to a string
        var serialized = JsonSerializer.Serialize(this);
        // Get the hash code of the serialized string
        return serialized.GetHashCode();
    }


    private void EnsureValidDetails()
    {
        var exception = new EmployeeDtoValidationException();
        if (string.IsNullOrWhiteSpace(Name))
        {
            exception.AddError(nameof(Name), "Name is required.");
        }
        if (Age < 18)
        {
            exception.AddError(nameof(Age), "Employee must be at least 18 years old.");
        }
        if (string.IsNullOrEmpty(State))
            exception.AddError(nameof(State), "State is required.");
        if (string.IsNullOrEmpty(Country))
            exception.AddError(nameof(Country), "Country is required.");
        if (Department == EmployeeDepartmentEnum.Unknown)
            exception.AddError(nameof(Department), "Department is required.");
        exception.ThrowIfErrors();
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
        if ((Age < 18))
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
    /// Employee Gender
    /// </summary>
    [JsonPropertyName("gender")]
    public GenderEnum Gender { get; set; }

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

    /// <summary>
    /// Profile Picture
    /// </summary>
    [Required(ErrorMessage = "Please choose profile image")]
    [JsonPropertyName("profile_picture")]
    public string? ProfilePicture { get; set; }
    public IFormFile? ProfileImage { get; set; }

}
