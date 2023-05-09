namespace Mwh.Sample.Repository.Models;
public interface IEmployee
{
    int Age { get; set; }
    string? Country { get; set; }
    Department Department { get; set; }
    int DepartmentId { get; set; }
    int? ManagerId { get; set; }
    string Name { get; set; }
    string? ProfilePicture { get; set; }
    string? State { get; set; }
}
