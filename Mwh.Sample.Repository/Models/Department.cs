
namespace Mwh.Sample.Repository.Models;

/// <summary>
/// 
/// </summary>
public class Department : BaseEntity, IDepartment
{
    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public ICollection<Employee>? Employees { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; }
}
