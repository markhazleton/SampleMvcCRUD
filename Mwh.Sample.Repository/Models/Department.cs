
namespace Mwh.Sample.Repository.Models;

/// <summary>
/// 
/// </summary>
public class Department : BaseEntity, IDepartment
{
    /// <summary>
    /// 
    /// </summary>
    public Department()
    {
        Name = string.Empty;
        Description = string.Empty;
        Employees = new HashSet<Employee>();
    }

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public virtual ICollection<Employee> Employees { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; }
}
