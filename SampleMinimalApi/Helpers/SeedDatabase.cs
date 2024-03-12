
using Mwh.Sample.Domain.Models;
using Mwh.Sample.Repository.Models;
using Mwh.Sample.Repository.Repository;
using Mwh.Sample.Repository.Services;

namespace SampleMinimalApi.Helpers;

/// <summary>
/// See Employee Database
/// </summary>
public static class SeedDatabase
{
    /// <summary>
    /// ConfirmDatabaseCreation
    /// </summary>
    public static async void DatabaseInitialization(EmployeeContext context)
    {
        try
        {
            using var employeeService = new EmployeeDatabaseService(context);
            var token = new CancellationToken();
            var employeeMock = new EmployeeMock(290);
            var deptResultList = new List<DepartmentResponse>();
            foreach (var dept in employeeMock.DepartmentCollection())
            {
                deptResultList.Add(await employeeService.SaveAsync(dept, token).ConfigureAwait(true));
            }
            var d = await employeeService.GetDepartmentsAsync(true, token).ConfigureAwait(true);
            var departmentCount = d.Count();
            employeeMock.EmployeeCollection()?.ForEach(async emp =>
            {
                await employeeService.SaveAsync(emp, token).ConfigureAwait(true);
            });
            var e = await employeeService.GetEmployeesAsync(new PagingParameterModel(), token).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            var exceptionMessage = ex.Message;
        }
    }
}
