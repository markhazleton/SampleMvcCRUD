
namespace Mwh.Sample.Repository.Services
{
    public class EmployeeDatabaseClient : IDisposable, IEmployeeClient
    {
        private readonly IEmployeeService service;

        public EmployeeDatabaseClient(IEmployeeService service)
        {
            this.service = service;
        }

        public async Task<int> AddMultipleEmployeesAsync(string?[]? namelist)
        {
            return await service.AddMultipleEmployeesAsync(namelist);
        }

        public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
        {
            return await service.DeleteAsync(id, token);
        }

        public void Dispose()
        {
            ((IDisposable)service).Dispose();
        }

        public async Task<DepartmentDto> FindDepartmentByIdAsync(int id, CancellationToken token)
        {
            return await FindDepartmentByIdAsync(id, token);
        }

        public async Task<EmployeeResponse> FindEmployeeByIdAsync(int id, CancellationToken token)
        {
            return await service.FindEmployeeByIdAsync(id, token);
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(bool IncludeEmployees, CancellationToken token)
        {
            return await service.GetDepartmentsAsync(IncludeEmployees, token);
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(PagingParameterModel paging, CancellationToken token)
        {
            return await service.GetEmployeesAsync(paging, token);
        }

        public async Task<EmployeeResponse> SaveAsync(EmployeeDto? employee, CancellationToken token)
        {
            return await service.SaveAsync(employee, token);
        }

        public async Task<DepartmentResponse> SaveAsync(DepartmentDto dept, CancellationToken token)
        {
            return await service.SaveAsync(dept, token);
        }

        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeDto? employee, CancellationToken token)
        {
            return await service.UpdateAsync(id, employee, token);
        }
    }
}
