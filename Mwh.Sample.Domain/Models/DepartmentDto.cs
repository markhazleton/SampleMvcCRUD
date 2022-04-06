
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
        public DepartmentDto()
        {
            Name = string.Empty;
            Description = string.Empty;
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
        public virtual EmployeeDto[]? Employees { get; set; }
    }
}
