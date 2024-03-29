﻿
using System.ComponentModel.DataAnnotations.Schema;

namespace Mwh.Sample.Repository.Models;
public class Employee : BaseEntity, IEmployee
{
    public Employee()
    {
        Name = string.Empty;
        ProfilePicture = "default.jpg";
    }

    public Employee(Department dbDept)
    {
        this.Department = dbDept;
        ProfilePicture = "default.jpg";
    }

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
    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    /// <summary>
    /// Gets or sets the department.
    /// </summary>
    /// <value>The department.</value>
    public int? DepartmentId { get; set; }

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
    /// <summary>
    /// Employee Manager Id
    /// </summary>
    public int? ManagerId { get; set; }
    /// <summary>
    /// Profile Picture
    /// </summary>
    public string? ProfilePicture { get; set; }
    /// <summary>
    /// Employee Gender
    /// </summary>
    public Gender Gender { get; set; }
}
