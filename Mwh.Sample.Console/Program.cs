CancellationToken cancellationToken = new CancellationToken();

Console.WriteLine("Setup SQL Lite Database");

var options = new DbContextOptionsBuilder<EmployeeContext>()
    .UseSqlite(@"Data Source=EmployeeConsole.db")
    .EnableSensitiveDataLogging(true)
    .Options;
using var context = new EmployeeContext(options);
await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

Console.WriteLine("Database is Setup");

var employeeService = new EmployeeDatabaseService(context);

var employees = await employeeService.GetEmployeesAsync(new CancellationToken()).ConfigureAwait(false);

Console.WriteLine($"Service List Count:{employees?.Count()}");

Console.WriteLine("Create MOCK to get sample Employees");
var employeeMock = new EmployeeMock();

var employeeList = new List<EmployeeResponse>();
var departmentList = new List<DepartmentResponse>();

Console.WriteLine("Add sample Departments to new database");
employeeMock.DepartmentCollection()?.ForEach(async dept =>
{
    var dep = await employeeService.SaveDepartmentAsync(dept).ConfigureAwait(false);
    departmentList.Add(dep);
});
Console.WriteLine($"Success List Count:{departmentList?.Where(w => w.Success == true).ToArray().Length}");

Console.WriteLine("Add sample Employees to new database");
employeeMock.EmployeeCollection()?.ForEach(async emp =>
{
    employeeList.Add(await employeeService.SaveAsync(emp, cancellationToken).ConfigureAwait(false));
});

Console.WriteLine($"\n\nSuccess List Count:{employeeList?.Where(w => w.Success == true).ToArray().Length}");

employees = await employeeService.GetEmployeesAsync(cancellationToken).ConfigureAwait(false);

var departments = await employeeService.GetDepartmentsAsync(cancellationToken).ConfigureAwait(false);

foreach (var dept in departments)
{
    Console.WriteLine($"{dept.Name} with {dept?.Employees?.Length ?? 0} employees");
}

Console.WriteLine($"\n\nEmployee Count:{employees?.Count()}");
Console.WriteLine("Complete");
Console.ReadKey();
