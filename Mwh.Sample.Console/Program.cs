CancellationToken ct = new CancellationToken();

Console.WriteLine("Setup SQL Lite Database");

var options = new DbContextOptionsBuilder<EmployeeContext>()
    .UseSqlite(@"Data Source=EmployeeConsole.db")
    .EnableSensitiveDataLogging(true)
    .Options;
using var context = new EmployeeContext(options);
try
{
    await context.Database.EnsureDeletedAsync().ConfigureAwait(false);
    await context.Database.EnsureCreatedAsync().ConfigureAwait(false);
    Console.WriteLine("Database is Setup");
}
catch (Exception ex)
{
    Console.WriteLine("Database Initialization error");
    Console.WriteLine(ex.ToString());
}

var employeeService = new EmployeeDatabaseService(context);

var employees = await employeeService.GetEmployeesAsync(new PagingParameterModel(), new CancellationToken()).ConfigureAwait(false);

Console.WriteLine($"Service List Count:{employees?.Count()}");

Console.WriteLine("Create MOCK to get sample Employees");
var employeeMock = new EmployeeMock(100);
var employeeList = new List<EmployeeResponse>();
var departmentList = new List<DepartmentResponse>();

try
{
    Console.WriteLine("Add sample Departments to new database");
    employeeMock.DepartmentCollection()?.ForEach(async dept =>
    {
        var dep = await employeeService.SaveDepartmentAsync(dept).ConfigureAwait(false);
        departmentList.Add(dep);
    });
    Console.WriteLine($"Department Success Count:{departmentList?.Where(w => w.Success == true).ToArray().Length}");

    Console.WriteLine("Add sample Employees to new database");
    employeeMock.EmployeeCollection()?.ForEach(async emp =>
    {
        employeeList.Add(await employeeService.SaveAsync(emp, ct).ConfigureAwait(false));
    });
    Console.WriteLine($"Employee Success Count:{employeeList?.Where(w => w.Success == true).ToArray().Length}");
}
catch (Exception ex)
{
    Console.WriteLine("Database Load error");
    Console.WriteLine(ex.ToString());
}


employees = await employeeService.GetEmployeesAsync(new PagingParameterModel(), ct).ConfigureAwait(false);
var departments = await employeeService.GetDepartmentsAsync(ct).ConfigureAwait(false);
foreach (var dept in departments)
{
    Console.WriteLine($"{dept.Name} with {dept?.Employees?.Length ?? 0} employees");
}
Console.WriteLine($"\n\nEmployee Count:{employees?.Count()}");
Console.WriteLine("Complete");
Console.ReadKey();
