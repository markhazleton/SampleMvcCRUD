
namespace Mwh.Sample.Domain.Models
{

    /// <summary>
    /// 
    /// </summary>
    public record class DepartmentDto
    {
        /// <summary>
        /// 
        /// </summary>
        public DepartmentDto(int id, string name, string? description = null)
        {
            ValidateName(name, nameof(name));
            ValidateId(id, nameof(id));

            Id = id;
            _name = name;
            Description = description;
        }
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; }
        /// <summary>
        /// 
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                ValidateName(value, nameof(Name));
                _name = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string? Description { get; set; }
        public virtual EmployeeDto?[]? Employees { get; set; }

        public static void ValidateName(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Department name cannot be null or whitespace", paramName);
        }
        public static void ValidateId(int Id, string paramName)
        {
            if (Id <= 0)
                throw new ArgumentException("Department Id must be greater than zero", paramName);
        }

        private static ArgumentException BuildInvalidIdException(int value, string paramName) =>
            new($"Department ID must be >0. Actual value was: {value}", paramName);

        public override string ToString() => $"Department Id={Id}, Name={Name}";




    }
}
