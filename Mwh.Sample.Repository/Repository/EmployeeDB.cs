﻿
using Mwh.Sample.Domain.Extensions;

namespace Mwh.Sample.Repository.Repository;

public class EmployeeDB : IEmployeeDB
{
    private EmployeeContext _context;

    public EmployeeDB(EmployeeContext context)
    {
        _context = context;
    }

    private static List<EmployeeDto> Create(List<Employee> list)
    {
        if (list == null) return new List<EmployeeDto>();
        return list.Select(item => Create(item)).OrderBy(x => x.Name).ToList();
    }
    private static List<DepartmentDto> Create(List<Department> list)
    {
        List<DepartmentDto> returnList = new();

        if (list == null) return returnList;

        returnList.AddRange(list.Where(w => !string.IsNullOrEmpty(w?.Name)).Select(item => Create(item)).OrderBy(x => x?.Name).ToList());

        return returnList ?? new List<DepartmentDto>();
    }
    private static DepartmentDto Create(Department item)
    {
        if (item == null)
            throw new ArgumentException("Department can not be null");

        EmployeeDepartmentEnum enumDept = (EmployeeDepartmentEnum)item.Id;

        DepartmentDto dept = new DepartmentDto
        {
            Id = item.Id,
            Name = enumDept.GetDisplayName(),
            Description = enumDept.GetDescription(),
            Employees = item?.Employees?.Select(s => Create(s)).ToArray()
        };

        return dept;
    }

    private static EmployeeDto Create(Employee entity)
    {
        return new EmployeeDto(
            entity.Id,
            entity.Name ?? "DEFAULT",
            entity.Age,
            entity.State ?? "TX",
            entity.Country ?? "USA",
            (EmployeeDepartmentEnum)entity.DepartmentId,
            entity.ProfilePicture ?? "default.jpg",
            (GenderEnum)entity.Gender
        );
    }

    public async Task<bool> DeleteEmployeeAsync(int ID)
    {
        Employee? delEmployee = await _context.Employees.FindAsync(ID);
        if (delEmployee is null)
        {
            return false;
        }
        try
        {
            _context.Employees.Remove(delEmployee);
            await _context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        return true;
    }

    public async Task<DepartmentDto> DepartmentAsync(int id)
    {
        return Create(await _context.Departments.Where(w => w.Id == id).Include(i => i.Employees).FirstOrDefaultAsync());
    }

    public async Task<List<DepartmentDto>> DepartmentCollectionAsync()
    {
        try
        {
            List<Department> dbDeptList = await _context.Departments.OrderBy(o => o.Name).ToListAsync();
            return Create(dbDeptList);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<EmployeeDto?> EmployeeAsync(int id)
    {
        Employee? empEntity = await _context.Employees.Where(w => w.Id == id).FirstOrDefaultAsync();
        return empEntity is null ? null : Create(empEntity);
    }

    public async Task<List<EmployeeDto>> EmployeeCollectionAsync()
    {
        return Create(await _context.Employees.OrderBy(o => o.Name).ToListAsync());
    }

    public async Task<EmployeeDto?> UpdateAsync(EmployeeDto? emp)
    {
        if (emp == null) return null;

        if (emp.Id == 0)
        {
            Employee saveUser = new Employee()
            {
                Name = emp.Name,
                State = emp.State,
                Age = emp.Age,
                Country = emp.Country,
                DepartmentId = (int)emp.Department,
                ProfilePicture = emp.ProfilePicture ?? "default.jpg",
                Gender = (Gender)emp.Gender
            };
            await _context.Employees.AddAsync(saveUser);
            await _context.SaveChangesAsync();
            emp.Id = saveUser.Id;
        }
        else
        {
            Employee? saveUser = await _context.Employees.FindAsync(emp.Id);

            if (saveUser != null)
            {
                _context.Attach(saveUser);
                saveUser.Name = emp.Name;
                saveUser.State = emp.State;
                saveUser.Age = emp.Age;
                saveUser.Country = emp.Country;
                saveUser.DepartmentId = (int)emp.Department;
                saveUser.ProfilePicture = emp.ProfilePicture ?? "default.jpg";
                saveUser.LastUpdatedDate = DateTime.Now;
                saveUser.Gender = (Gender)emp.Gender;
                await _context.SaveChangesAsync();
            }
            else
            {
                saveUser = new Employee()
                {
                    Id = emp.Id,
                    Name = emp.Name,
                    State = emp.State,
                    Age = emp.Age,
                    Country = emp.Country,
                    DepartmentId = (int)emp.Department,
                    ProfilePicture = emp.ProfilePicture ?? "default.jpg",
                    Gender = (Gender)emp.Gender,
                };
                await _context.Employees.AddAsync(saveUser);
                await _context.SaveChangesAsync();
                emp.Id = saveUser.Id;

            }
        }
        return await EmployeeAsync(emp.Id);
    }

    public async Task<DepartmentDto?> UpdateAsync(DepartmentDto? dept)
    {
        if (dept == null) return null;

        if (dept?.Id == 0)
        {
            return null;
        }

        DepartmentDto updateDept = new()
        {
            Id = dept.Id,
            Name = dept.Name,
            Description = dept.Description
        };

        Department? saveDept = await _context.Departments.FindAsync(updateDept.Id);
        if (saveDept != null)
        {
            _context.Departments.Attach(saveDept);
            saveDept.Name = string.IsNullOrEmpty(updateDept.Name) ? saveDept.Name : updateDept.Name;
            saveDept.Description = string.IsNullOrEmpty(updateDept.Description) ? saveDept.Description : updateDept.Description;
            await _context.SaveChangesAsync();
        }
        else
        {
            Department newDept = new Department()
            {
                Id = updateDept.Id,
                Name = string.IsNullOrEmpty(updateDept.Name) ? "MISSING NAME" : updateDept.Name,
                Description = string.IsNullOrEmpty(updateDept.Description) ? "MISSING DESCRIPTION" : updateDept.Description,
            };
            await _context.Departments.AddAsync(newDept);
            await _context.SaveChangesAsync();
        }
        return await DepartmentAsync(updateDept.Id);
    }
}
