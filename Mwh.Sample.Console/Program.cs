CancellationToken ct = new CancellationToken();

Console.WriteLine("Setup SQL Lite Database");

DbContextOptions<EmployeeContext> options = new DbContextOptionsBuilder<EmployeeContext>()
    .UseSqlite(@"Data Source=EmployeeConsole.db")
    .EnableSensitiveDataLogging(true)
    .Options;
using EmployeeContext context = new EmployeeContext(options);
try
{
    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();
    Console.WriteLine("Database is Setup");
}
catch (Microsoft.Data.Sqlite.SqliteException ex)
{
    Console.WriteLine("SQLite Database Initialization error");
    Console.WriteLine(ex.ToString());
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Database Initialization error");
    Console.WriteLine(ex.ToString());
}

EmployeeDatabaseService employeeService = new EmployeeDatabaseService(context);

IEnumerable<EmployeeDto> employees = await employeeService.GetEmployeesAsync(new PagingParameterModel(), new CancellationToken());

Console.WriteLine($"Service List Count:{employees?.Count()}");

Console.WriteLine("Create MOCK to get sample Employees");
EmployeeMock employeeMock = new EmployeeMock(100);
List<EmployeeResponse> employeeList = new List<EmployeeResponse>();
List<DepartmentResponse> departmentList = new List<DepartmentResponse>();

try
{
    Console.WriteLine("Add sample Departments to new database");
    employeeMock.DepartmentCollection()?.ForEach(async dept =>
    {
        DepartmentResponse dep = await employeeService.SaveDepartmentAsync(dept);
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
catch (DbUpdateException ex)
{
    Console.WriteLine("Database Update error");
    Console.WriteLine(ex.ToString());
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Database Load error");
    Console.WriteLine(ex.ToString());
}


employees = await employeeService.GetEmployeesAsync(new PagingParameterModel(), ct);
IEnumerable<DepartmentDto> departments = await employeeService.GetDepartmentsAsync(true, ct);
foreach (DepartmentDto dept in departments)
{
    Console.WriteLine($"{dept.Name} with {dept?.Employees?.Length ?? 0} employees");
}
Console.WriteLine($"\n\nEmployee Count:{employees?.Count()}");
Console.WriteLine("Complete");
Console.ReadKey();
