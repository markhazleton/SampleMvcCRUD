
namespace Mwh.Sample.Domain.Models
{

    /// <summary>
    /// 
    /// </summary>
    public class DepartmentDto
    {
        /// <summary>
        /// 
        /// </summary>
        public DepartmentDto(int id, string name, string? description = null)
        {
            ValidateName(name, nameof(name));
            if (id <= 0)
                throw BuildInvalidIdException(id, nameof(id));
            Id = id;
            Name = name;
            Description = description ?? string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        public virtual EmployeeDto?[]? Employees { get; set; }

        public void ValidateName(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Department name cannot be null or whitespace", paramName);
        }

        private ArgumentException BuildInvalidIdException(int value, string paramName) =>
            new ArgumentException($"Department ID must be >0. Actual value was: {value}", paramName);

        public override string ToString() => $"Department Id={Id}, Name={Name}";




    }
}
