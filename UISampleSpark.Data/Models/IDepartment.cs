namespace UISampleSpark.Data.Models;

public interface IDepartment
{
    string Description { get; set; }
    ICollection<Employee> Employees { get; set; }
    string Name { get; set; }
}
