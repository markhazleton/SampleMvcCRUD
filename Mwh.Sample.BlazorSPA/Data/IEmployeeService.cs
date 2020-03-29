using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mwh.Sample.BlazorSPA.Data
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetEmployees();
        Task<bool> CreateEmployee(Employee employee);
        Task<bool> EditEmployee(string id, Employee employee);
        Task<Employee> SingleEmployee(string id);
        Task<bool> DeleteEmployee(string id);
    }
}
