namespace Mwh.Sample.Domain.Models;

/// <summary>
/// Employee List
/// </summary>
public class EmployeeList
{
    private readonly List<EmployeeDto> _list = new();

    public EmployeeList AddItem(EmployeeDto? item)
    {
        if (item == null) return this;
        _list.Add(item);
        return this;
    }
    public List<EmployeeDto> List => _list;


    public IEnumerable<EmployeeDto> EnumerateItems()
    {
        foreach (var item in _list)
            yield return item;
    }
}
