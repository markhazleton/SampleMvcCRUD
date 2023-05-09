using Microsoft.AspNetCore.Http;

namespace Mwh.Sample.Domain.Models;

public interface IEmployeeDto
{
    bool Equals(object obj);
    int GetHashCode();
    bool IsValid();
    int Age { get; set; }
    string? Country { get; set; }
    EmployeeDepartmentEnum Department { get; set; }
    string? DepartmentName { get; set; }
    int Id { get; set; }
    int? ManagerId { get; set; }
    string? Name { get; set; }
    IFormFile? ProfileImage { get; set; }
    string? ProfilePicture { get; set; }
    string? State { get; set; }
}
