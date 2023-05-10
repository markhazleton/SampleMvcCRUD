namespace Mwh.Sample.Repository.Models;

public interface IDepartment
{
    string Description { get; set; }
    ICollection<Employee> Employees { get; set; }
    string Name { get; set; }
}
