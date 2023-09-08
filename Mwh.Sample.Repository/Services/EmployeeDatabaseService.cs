namespace Mwh.Sample.Repository.Services;
public class EmployeeDatabaseService : IDisposable, IEmployeeService
{
    private readonly EmployeeContext _context;


    public EmployeeDatabaseService(EmployeeContext context)
    {
        _context = context;
    }

    private static DepartmentDto? Create(Department? item)
    {
        if (item == null) return null;

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
            Description = item?.Description ?? string.Empty,
        };
    }
    private static EmployeeDto? Create(Employee? item)
    {
        if (item == null) return null;

        var retEmployee = new EmployeeDto
        {
            Id = item.Id,
            Name = item.Name,
            Age = item.Age,
            State = item.State,
            Country = item.Country,
            Department = (EmployeeDepartmentEnum)(item?.Department?.Id ?? 0),
            ProfilePicture = item.ProfilePicture,
            Gender = (GenderEnum)(item?.Gender ?? 0),
            DepartmentName = item?.Department?.Name ?? string.Empty,
            GenderName = ((GenderEnum)(item?.Gender ?? 0)).ToString()
        };

        return retEmployee;
    }

    private static Employee Create(EmployeeDto item, Department dbDept)
    {
        return new Employee()
        {
            DepartmentId = dbDept.Id,
            Name = item.Name,
            State = item.State,
            Country = item.Country,
            Age = item.Age,
            ProfilePicture = item.ProfilePicture ?? "default.jpg",
            Gender = (Gender)item.Gender,
            Id = item.Id
        };
    }
    private static DepartmentDto[] GetDepartmentDtos(List<Department> list)
    {
        if (list is null) return Array.Empty<DepartmentDto>();
        return list?.Select(s => Create(s)).OfType<DepartmentDto>().ToArray() ?? Array.Empty<DepartmentDto>();
    }

    private static EmployeeDto[] GetEmployeeDtos(List<Employee> list)
    {
        if (list is null) return Array.Empty<EmployeeDto>();
        return list?.Select(s => Create(s)).OfType<EmployeeDto>().ToArray() ?? Array.Empty<EmployeeDto>();
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

        Random rand = new();
        var list = new List<Employee>();
        foreach (var name in namelist)
        {
            list.Add(new Employee()
            {
                Name = name ?? "UNKNOWN",
                Age = rand.Next(18, 100),
                Country = "USA",
                DepartmentId = rand.Next(1, (Enum.GetNames(typeof(EmployeeDepartmentEnum)).Length - 1)),
                Gender = (Gender)rand.Next(1, (Enum.GetNames(typeof(Gender)).Length - 1)),
                State = "TX"
            });
        }

        await _context.Employees.AddRangeAsync(list).ConfigureAwait(true);
        await _context.SaveChangesAsync().ConfigureAwait(true);

        return _context.Employees.Count();
    }

    public async Task<EmployeeResponse> DeleteAsync(int id, CancellationToken ct)
    {
        var response = new EmployeeResponse("Init");

        if (id > 0)
        {
            var dbEmp = await _context.Employees.FindAsync(id);

            if (dbEmp == null)
            {
                response = new EmployeeResponse("Employee Not Found");
            }
            else
            {
                _context.Employees.Remove(dbEmp);
                await _context.SaveChangesAsync(ct)
                    .ConfigureAwait(true);

                return new EmployeeResponse(true);
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
        var employee = Create(await _context.Employees.Include(i=>i.Department).Where(w=>w.Id==Id).FirstOrDefaultAsync(cancellationToken: token));
        if (employee is null)
            return new EmployeeResponse("Employee Not Found");

        return new EmployeeResponse(employee);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsAsync(bool IncludeEmployees, CancellationToken token)
    {
        if (IncludeEmployees)
            return GetDepartmentDtos(await _context.Departments.Where(w => !string.IsNullOrEmpty(w.Name)).Include(e => e.Employees).ToListAsync(cancellationToken: token));

        return GetDepartmentDtos(await _context.Departments.Where(w => !string.IsNullOrEmpty(w.Name)).ToListAsync(cancellationToken: token));
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(PagingParameterModel paging, CancellationToken token)
    {
        var source = _context.Employees.Include(i=>i.Department).OrderBy(o => o.Name).AsQueryable();
        int TotalCount = source.Count();
        var items = await source.Skip((paging.PageNumber - 1) * paging.PageSize).Take(paging.PageSize).ToListAsync(token);
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
        if (item is null)
            return new DepartmentResponse("Department can not be null");

        Department? dbDept;
        if (item.Id > 0)
        {
            dbDept = await _context.Departments.FindAsync(item.Id);
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
    public async Task<EmployeeResponse> SaveEmployeeDbAsync(EmployeeDto? newItem, CancellationToken ct = default)
    {
        EmployeeDto item;
        if (newItem is null)
        {
            return new EmployeeResponse("Employee can not be null");
        }
        try
        {
            item = new EmployeeDto(
                newItem.Id,
                newItem.Name,
                newItem.Age,
                newItem.State,
                newItem.Country,
                newItem.Department,
                newItem.ProfilePicture,
                newItem.Gender);

            int deptId = (int)item.Department;
            var dbDept = _context.Departments.Find(deptId);

            if (dbDept == null)
                return new EmployeeResponse("Department not found");

            Employee? dbEmp;

            if (item.Id > 0)
            {
                dbEmp = await _context.Employees.Include(d=>d.Department).Where(w=>w.Id==item.Id).FirstOrDefaultAsync(cancellationToken: ct).ConfigureAwait(true);

                if (dbEmp == null)
                {
                    dbEmp = Create(item,dbDept);

                    _context.Employees.Add(dbEmp);
                    _context.SaveChanges();
                }
                else
                {
                    dbEmp.Age = item.Age;
                    dbEmp.Country = item.Country;
                    dbEmp.DepartmentId = dbDept.Id;
                    dbEmp.Department = dbDept;
                    dbEmp.Name = item.Name ?? string.Empty;
                    dbEmp.State = item.State;
                    dbEmp.LastUpdatedDate = DateTime.Now;
                    dbEmp.ProfilePicture = item.ProfilePicture ?? "default.jpg";
                    dbEmp.Gender = (Gender)item.Gender;
                    _context.Update(dbEmp);
                    _context.SaveChanges();
                }
            }
            else
            {
                dbEmp = Create(item, dbDept);
                
                var desalt = _context.Employees.Add(dbEmp);
                _context.SaveChanges();
            }

            var newEmp = await _context.Employees.Where(w => w.Id == dbEmp.Id).Include(i => i.Department).FirstOrDefaultAsync(cancellationToken: ct);

            return new EmployeeResponse(Create(newEmp));
        }
        catch (Exception ex)
        {
            return new EmployeeResponse($"Exception:{ex.Message}");
        }
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


        return await SaveAsync(employee, token);
    }
}
