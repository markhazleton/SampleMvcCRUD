
namespace Mwh.Sample.Domain.Models;

/// <summary>
/// Class EmployeeResponse.
/// </summary>
public class EmployeeResponse : BaseResponse<EmployeeDto>
{
    public EmployeeResponse() { }
    /// <summary>
    /// Creates a success response.
    /// </summary>
    /// <param name="employee">Saved employee.</param>
    /// <returns>Response.</returns>
    public EmployeeResponse(EmployeeDto? employee) : base(employee) { }

    /// <summary>
    /// Creates am error response.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <returns>Response.</returns>
    public EmployeeResponse(string message) : base(message) { }
}
