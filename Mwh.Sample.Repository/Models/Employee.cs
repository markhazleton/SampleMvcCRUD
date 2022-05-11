
namespace Mwh.Sample.Repository.Models;
public class Employee : BaseEntity
{
    /// <summary>
    /// Gets or sets the age.
    /// </summary>
    /// <value>The age.</value>
    public int Age { get; set; }

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    /// <value>The country.</value>
    public string? Country { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public virtual Department Department { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    /// <value>The department.</value>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    /// <value>The state.</value>
    public string? State { get; set; }
}
