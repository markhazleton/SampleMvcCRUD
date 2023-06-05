namespace Mwh.Sample.Domain.Models;


/// <summary>
/// Represents a data transfer object (DTO) for a department.
/// </summary>
public record class DepartmentDto
{
    private string _name;

    public DepartmentDto()
    {
        Id = 0;
        _name = EmployeeDepartmentEnum.Unknown.ToString();
        Description = EmployeeDepartmentEnum.Unknown.GetDescription();
    }

    public DepartmentDto(EmployeeDepartmentEnum enumValue)
    {
        Id = (int)enumValue;
        _name = enumValue.ToString();
        Description = enumValue.GetDescription();
    }

    public string Description { get; set; }

    public EmployeeDto[] Employees { get; set; }

    [Key]
    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            ValidateName(value, nameof(Name));
            _name = value;
        }
    }

    public override string ToString()
    {
        return $"Department Id={Id}, Name={Name}";
    }

    public static void ValidateId(int id, string paramName)
    {
        if (id <= 0)
            throw new ArgumentException("Department Id must be greater than zero", paramName);
    }

    public static void ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name cannot be null or whitespace", paramName);
    }
}
