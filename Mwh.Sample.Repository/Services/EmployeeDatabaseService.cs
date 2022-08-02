
namespace Mwh.Sample.Repository.Services;
public class EmployeeDatabaseService : IDisposable, IEmployeeService
{
    private readonly EmployeeContext _context;


    public EmployeeDatabaseService(EmployeeContext context)
    {
        _context = context;
    }

    private static DepartmentDto Create(Department? item)
    {
        if (item == null) return new DepartmentDto();

        return new DepartmentDto()
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Employees = item?.Employees?.Select(s => Create(s)).ToArray() ?? null
        };
    }

    private static Department Create(DepartmentDto item)
    {
        return new Department()
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
        };
    }
    private static EmployeeDto? Create(Employee? item)
    {
        if (item == null) return null;

        return new EmployeeDto()
        {
            Name = item.Name,
            State = item?.State ?? string.Empty,
            Country = item?.Country ?? string.Empty,
            Age = item?.Age ?? 0,
            Department = (EmployeeDepartmentEnum)(item?.DepartmentId ?? 1),
            DepartmentName = ((EmployeeDepartmentEnum)(item?.DepartmentId ?? 0)).ToString() ?? string.Empty,
            Id = item?.Id ?? 0
        };
    }

    private static Employee Create(EmployeeDto item)
    {
        return new Employee()
        {
            Name = item.Name,
            State = item.State,
            Country = item.Country,
            Age = item.Age,
            DepartmentId = (int)item.Department,
            Id = item.Id
        };
    }
    private static DepartmentDto[] GetDepartmentDtos(List<Department> list)
    {
        return list.Select(s => Create(s)).ToArray();
    }

    private static EmployeeDto[] GetEmployeeDtos(List<Employee> list)
    {
        var returnList = list?.Select(s => Create(s)).ToArray();
        return returnList;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_context != null)
            {
                ((IDisposable)_context).Dispose();
            }
        }
    }

    public async Task<int> AddMultipleEmployeesAsync(string?[]? namelist)
    {
        if (namelist == null) return -1;

        var list = new List<Employee>();
        foreach (var name in namelist)
        {
            list.Add(new Employee() { Name = name ?? "UNKNOWN", Age = 33, Country = "USA", DepartmentId = 1, State = "TX" });
        }
        _context.Employees.AddRange(list);
        var dbResult = await _context.SaveChangesAsync();
        return dbResult;

    }

    public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken token)
    {
        var response = new EmployeeResponse("Init");

        if (id > 0)
        {
            var dbEmp = await _context.Employees
                .Where(w => w.Id == id)
                .FirstOrDefaultAsync(cancellationToken: token)
                .ConfigureAwait(true);

            if (dbEmp == null)
            {
                response = new EmployeeResponse("Employee Not Found");
            }
            else
            {
                _context.Employees.Remove(dbEmp);
                await _context.SaveChangesAsync(token)
                    .ConfigureAwait(true);

                return new EmployeeResponse(new EmployeeDto());
            }
        }
        else
        {
            return new EmployeeResponse("Invalid Employee Id for delete");
        }
        return response;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<DepartmentDto> FindDepartmentByIdAsync(int Id, CancellationToken token)
    { { return Create(await _context.Departments.Where(w => w.Id == Id).Include(i => i.Employees).FirstOrDefaultAsync(cancellationToken: token)); } }

    public async Task<EmployeeResponse> FindEmployeeByIdAsync(int Id, CancellationToken token)
    {
        var employee = Create(await _context.Employees.FindAsync(new object[] { Id }, cancellationToken: token).ConfigureAwait(false));
        if (employee is null)
            return new EmployeeResponse("Employee Not Found");

        return new EmployeeResponse(employee);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(CancellationToken token)
    {
        return GetDepartmentDtos(await _context.Departments.Include(e => e.Employees).ToListAsync(cancellationToken: token).ConfigureAwait(false));
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(PagingParameterModel paging, CancellationToken token)
    {
        var source = _context.Employees.OrderBy(o => o.Name).AsQueryable();
        int TotalCount = source.Count();
        var items = await source.Skip((paging.PageNumber - 1) * paging.PageSize).Take(paging.PageSize).ToListAsync(token).ConfigureAwait(false);
        var paginationMetadata = paging.GetMetaData(TotalCount);
        return GetEmployeeDtos(items);
    }

    public async Task<EmployeeResponse> SaveAsync(EmployeeDto? employee, CancellationToken token)
    {
        if (employee == null) return new EmployeeResponse("Employee can not be null");

        return await SaveEmployeeDbAsync(employee, token).ConfigureAwait(true);
    }

    public async Task<DepartmentResponse> SaveAsync(DepartmentDto dept, CancellationToken token)
    {
        if (dept == null) return new DepartmentResponse("Department can not be null");

        return await SaveDepartmentAsync(dept, token).ConfigureAwait(true);
    }

    public async Task<DepartmentResponse> SaveDepartmentAsync(DepartmentDto? item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return new DepartmentResponse("Department can not be null");

        Department? dbDept;
        if (item.Id > 0)
        {
            dbDept = await _context.Departments.Where(w => w.Id == item.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (dbDept == null)
            {
                dbDept = Create(item);
                _context.Departments.Add(dbDept);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                dbDept.Id = item.Id;
                dbDept.Name = item.Name;
                dbDept.Description = item.Description;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            return new DepartmentResponse("Zero ID not allowed");
        }
        return new DepartmentResponse(Create(dbDept));
    }
    public async Task<EmployeeResponse> SaveEmployeeDbAsync(EmployeeDto? newItem, CancellationToken cancellationToken = default)
    {
        if (newItem == null)
            return new EmployeeResponse("Employee can not be null");
        if (newItem?.Department == null)
            return new EmployeeResponse("Department can not be null");
        if ((newItem?.Department ?? 0) == 0)
            return new EmployeeResponse("Department can not be null or Zero");

        EmployeeDto item = newItem ?? new EmployeeDto();

        int deptId = (int)(item?.Department ?? 0);
        var dbDept = await _context.Departments.Where(w => w.Id == deptId).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (dbDept == null)
            return new EmployeeResponse("Department not found");

        var dbEmp = new Employee();

        try
        {
            if ((item?.Id ?? 0) > 0)
            {
                dbEmp = await _context.Employees
                    .Where(w => w.Id == item.Id)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(true);

                if (dbEmp == null)
                {
                    dbEmp = Create(item);
                    dbEmp.Department = dbDept;
                    await _context.Employees.AddAsync(dbEmp, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(true);
                }
                else
                {
                    dbEmp.Age = item.Age;
                    dbEmp.Country = item.Country;
                    dbEmp.DepartmentId = dbDept.Id;
                    dbEmp.Department = dbDept;
                    dbEmp.Name = item.Name;
                    dbEmp.State = item.State;
                    dbEmp.LastUpdatedDate = DateTime.Now;
                    _context.Update(dbEmp);
                    await _context.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(true);

                    var list = await _context.Employees
                        .ToListAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(true);
                }
            }
            else
            {
                dbEmp = Create(item);
                dbEmp.Department = dbDept;
                await _context.Employees.AddAsync(dbEmp, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            return new EmployeeResponse($"Exception:{ex.Message}");
        }

        var newEmp = await _context.Employees
            .Where(w => w.Id == dbEmp.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(true);

        return new EmployeeResponse(Create(newEmp ?? dbEmp));
    }

    public async Task<EmployeeResponse> UpdateAsync(int id, EmployeeDto? employee, CancellationToken token)
    {
        if (employee == null)
            return new EmployeeResponse($"Can not update null employee");

        if (employee.Id != id)
            return new EmployeeResponse($"Mismatch in id({id}) && id({employee.Id}).");

        if (employee.Id == 0)
            return new EmployeeResponse($"Can not update employee with id({id})");

        if (employee.Department == EmployeeDepartmentEnum.Unknown)
            return new EmployeeResponse($"Can not update employee with unknown department");


        return await SaveAsync(employee, token).ConfigureAwait(false);
    }
}
