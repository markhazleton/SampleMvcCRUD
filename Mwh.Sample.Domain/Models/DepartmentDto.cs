
namespace Mwh.Sample.Domain.Models;


/// <summary>
/// Represents a data transfer object (DTO) for a department.
/// </summary>
public record class DepartmentDto
{
    /// <summary>
    /// The name of the department.
    /// </summary>
    private string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="DepartmentDto"/> class.
    /// </summary>
    /// <param name="id">The ID of the department.</param>
    /// <param name="name">The name of the department.</param>
    /// <param name="description">The description of the department (optional).</param>
    public DepartmentDto(int id, string name, string? description = null)
    {
        ValidateName(name, nameof(name));
        ValidateId(id, nameof(id));

        Id = id;
        _name = name;
        Description = description;
    }

    /// <summary>
    /// Returns a string representation of the department.
    /// </summary>
    /// <returns>A string representing the department.</returns>
    public override string ToString() => $"Department Id={Id}, Name={Name}";

    /// <summary>
    /// Validates the ID of the department.
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <param name="paramName">The name of the parameter.</param>
    public static void ValidateId(int id, string paramName)
    {
        if (id <= 0)
            throw new ArgumentException("Department Id must be greater than zero", paramName);
    }

    /// <summary>
    /// Validates the name of the department.
    /// </summary>
    /// <param name="name">The name to validate.</param>
    /// <param name="paramName">The name of the parameter.</param>
    public static void ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name cannot be null or whitespace", paramName);
    }

    /// <summary>
    /// Gets or sets the description of the department.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the employees associated with the department.
    /// </summary>
    public virtual EmployeeDto?[]? Employees { get; set; }

    /// <summary>
    /// Gets the ID of the department.
    /// </summary>
    [Key]
    public int Id { get; }

    /// <summary>
    /// Gets or sets the name of the department.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            ValidateName(value, nameof(Name));
            _name = value;
        }
    }
}
