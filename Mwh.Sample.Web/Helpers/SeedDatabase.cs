using Mwh.Sample.Repository.Repository;

namespace Mwh.Sample.Web.Helpers;

/// <summary>
/// Helper class for seeding the employee database with initial test data
/// </summary>
public static class SeedDatabase
{
    /// <summary>
    /// Initializes the database with sample departments and employees
    /// </summary>
    /// <param name="context">The employee database context to seed</param>
    public static async void DatabaseInitialization(EmployeeContext context)
    {
        try
        {
            EmployeeDatabaseService employeeService = new EmployeeDatabaseService(context);
            CancellationToken token = new CancellationToken();
            EmployeeMock employeeMock = new EmployeeMock(290);
            List<DepartmentResponse> deptResultList = new List<DepartmentResponse>();

            // First add all departments
            foreach (DepartmentDto dept in employeeMock.DepartmentCollection())
            {
                deptResultList.Add(await employeeService.SaveAsync(dept, token).ConfigureAwait(true));
            }

            // Verify departments were added
            IEnumerable<DepartmentDto> d = await employeeService.GetDepartmentsAsync(true, token).ConfigureAwait(true);
            int departmentCount = d.Count();

            // Then add all employees
            employeeMock.EmployeeCollection()?.ForEach(async emp =>
            {
                await employeeService.SaveAsync(emp, token).ConfigureAwait(true);
            });

            // Verify employees were added
            IEnumerable<EmployeeDto> e = await employeeService.GetEmployeesAsync(new PagingParameterModel(), token).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            string exceptionMessage = ex.Message;
        }
    }
}
