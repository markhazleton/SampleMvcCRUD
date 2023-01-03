CancellationToken ct = new CancellationToken();

Console.WriteLine("Setup SQL Lite Database");

var options = new DbContextOptionsBuilder<EmployeeContext>()
    .UseSqlite(@"Data Source=EmployeeConsole.db")
    .EnableSensitiveDataLogging(true)
    .Options;
using var context = new EmployeeContext(options);
try
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("Database is Setup");
}
catch (Exception ex)
{
    Console.WriteLine("Database Initialization error");
    Console.WriteLine(ex.ToString());
}

var employeeService = new EmployeeDatabaseService(context);

var employees = await employeeService.GetEmployeesAsync(new PagingParameterModel(), new CancellationToken());

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
        var dep = await employeeService.SaveDepartmentAsync(dept);
        departmentList.Add(dep);
    });
    Console.WriteLine($"Department Success Count:{departmentList?.Where(w => w.Success == true).ToArray().Length}");

    Console.WriteLine("Add sample Employees to new database");
    employeeMock.EmployeeCollection()?.ForEach(async emp =>
    {
        employeeList.Add(await employeeService.SaveAsync(emp, ct));
    });
    Console.WriteLine($"Employee Success Count:{employeeList?.Where(w => w.Success == true).ToArray().Length}");
}
catch (Exception ex)
{
    Console.WriteLine("Database Load error");
    Console.WriteLine(ex.ToString());
}


employees = await employeeService.GetEmployeesAsync(new PagingParameterModel(), ct);
var departments = await employeeService.GetDepartmentsAsync(true, ct);
foreach (var dept in departments)
{
    Console.WriteLine($"{dept.Name} with {dept?.Employees?.Length ?? 0} employees");
}
Console.WriteLine($"\n\nEmployee Count:{employees?.Count()}");
Console.WriteLine("Complete");
Console.ReadKey();
