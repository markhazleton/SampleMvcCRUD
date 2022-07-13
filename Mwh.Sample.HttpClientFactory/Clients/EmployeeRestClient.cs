
namespace Mwh.Sample.HttpClientFactory.Clients;

/// <summary>
///
/// </summary>
public class EmployeeRestClient : RestClientBase, IEmployeeClient
{
    public EmployeeRestClient(IHttpContextAccessor context, IHttpClientFactory httpClientFactory)
        : base($"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{context.HttpContext.Request.PathBase}", "employee", httpClientFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeRestClient"/> class.
    /// </summary>
    /// <param name="apiPath">The API path.</param>
    /// <param name="appName">Name of the application.</param>
    public EmployeeRestClient(string apiPath, string appName, IHttpClientFactory httpClientFactory) : base(apiPath, appName, httpClientFactory)
    {
    }

    public async Task<int> AddMultipleEmployeesAsync(string?[]? namelist)
    {
        return 0;
    }

    /// <summary>
    /// delete as an asynchronous operation.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>EmployeeResponse.</returns>
    public async Task<EmployeeResponse?> DeleteAsync(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return await ExecuteAsync<EmployeeResponse>($"/api/employee/{id}", requestBody: null, HttpMethod.Delete, token);
    }

    public async Task<DepartmentDto?> FindDepartmentByIdAsync(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return await ExecuteAsync<DepartmentDto>($"/api/department/{id}", null, HttpMethod.Get, token);
    }

    /// <summary>
    /// find by identifier as an asynchronous operation.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>EmployeeModel.</returns>
    public async Task<EmployeeResponse?> FindEmployeeByIdAsync(int id, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var result = await ExecuteAsync<EmployeeResponse>($"/api/employee/{id}", null, HttpMethod.Get, token);

        return result;
    }

    public async Task<IEnumerable<DepartmentDto>?> GetDepartmentsAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return await ExecuteAsync<IEnumerable<DepartmentDto>>($"/api/department", null, HttpMethod.Get, token);
    }

    /// <summary>
    /// list as an asynchronous operation.
    /// </summary>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>IEnumerable&lt;EmployeeModel&gt;.</returns>
    public async Task<IEnumerable<EmployeeDto>?> GetEmployeesAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return await ExecuteAsync<IEnumerable<EmployeeDto>>($"/api/employee", null, HttpMethod.Get, token);
    }




    /// <summary>
    /// save as an asynchronous operation.
    /// </summary>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>EmployeeResponse.</returns>
    public async Task<EmployeeResponse?> SaveAsync(EmployeeDto? employee, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return await ExecuteAsync<EmployeeResponse>($"/api/employee", employee, HttpMethod.Post, token);
    }

    public async Task<DepartmentResponse?> SaveAsync(DepartmentDto dept, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return await ExecuteAsync<DepartmentResponse>($"/api/department", dept, HttpMethod.Post, token);
    }

    /// <summary>
    /// update as an asynchronous operation.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="employee">The employee.</param>
    /// <param name="token">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns>EmployeeResponse.</returns>
    public async Task<EmployeeResponse?> UpdateAsync(int id,
        EmployeeDto? employee,
        CancellationToken token)
    {
        if (employee is null)
            return new EmployeeResponse("Employee can not be null");
        token.ThrowIfCancellationRequested();
        if (employee.Id == id)
        {
            return await ExecuteAsync<EmployeeResponse>($"/api/employee/{id}", employee, HttpMethod.Put, token);
        }
        return await Task.Run(() =>
        {
            return new EmployeeResponse($"Mismatch in id({id}) && id({employee.Id}).");
        });
    }
}
