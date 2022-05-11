
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
            return await service.DeleteAsync(id, token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            ((IDisposable)service).Dispose();
        }

        public async Task<DepartmentDto> FindDepartmentByIdAsync(int id, CancellationToken token)
        {
            return await FindDepartmentByIdAsync(id, token).ConfigureAwait(false);  
        }

        public async Task<EmployeeResponse> FindEmployeeByIdAsync(int id, CancellationToken token)
        {
            return await service.FindEmployeeByIdAsync(id, token).ConfigureAwait(false);
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken token)
        {
            return await service.GetDepartmentsAsync(token).ConfigureAwait(false);
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(CancellationToken token)
        {
            return await service.GetEmployeesAsync(token).ConfigureAwait(false);
        }

        public async Task<EmployeeResponse> SaveAsync(EmployeeDto employee, CancellationToken token)
        {
            return await service.SaveAsync(employee, token).ConfigureAwait(false);
        }

        public async Task<DepartmentResponse> SaveAsync(DepartmentDto dept, CancellationToken token)
        {
            return await service.SaveAsync(dept, token).ConfigureAwait(false);
        }

        public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeDto? employee, CancellationToken token)
        {
            return await service.UpdateAsync(id, employee, token).ConfigureAwait(false);
        }
    }
}
