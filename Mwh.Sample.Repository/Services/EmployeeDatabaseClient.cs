namespace Mwh.Sample.Repository.Services;

public class EmployeeDatabaseClient : IEmployeeClient
{
    private readonly IEmployeeService service;

    public EmployeeDatabaseClient(IEmployeeService service)
    {
        this.service = service;
    }

    public async Task<int> AddMultipleEmployeesAsync(string[] namelist)
    {
        return await service.AddMultipleEmployeesAsync(namelist).ConfigureAwait(false);
    }

    public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
    {
        return await service.DeleteAsync(id, token).ConfigureAwait(false);
    }

    public async Task<DepartmentDto> FindDepartmentByIdAsync(int id, CancellationToken token)
    {
        return await service.FindDepartmentByIdAsync(id, token).ConfigureAwait(false);
    }

    public async Task<EmployeeResponse> FindEmployeeByIdAsync(int id, CancellationToken token)
    {
        return await service.FindEmployeeByIdAsync(id, token).ConfigureAwait(false);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(bool includeEmployees, CancellationToken token)
    {
        return await service.GetDepartmentsAsync(includeEmployees, token).ConfigureAwait(false);
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(PagingParameterModel paging, CancellationToken token)
    {
        return await service.GetEmployeesAsync(paging, token).ConfigureAwait(false);
    }

    public async Task<EmployeeResponse> SaveAsync(EmployeeDto employee, CancellationToken token)
    {
        return await service.SaveAsync(employee, token).ConfigureAwait(false);
    }

    public async Task<DepartmentResponse> SaveAsync(DepartmentDto dept, CancellationToken token)
    {
        return await service.SaveAsync(dept, token).ConfigureAwait(false);
    }

    public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeDto employee, CancellationToken token)
    {
        return await service.UpdateAsync(id, employee, token).ConfigureAwait(false);
    }
}
