
namespace Mwh.Sample.Domain.Models
{
    public class DepartmentResponse : BaseResponse<DepartmentDto>
    {
        public DepartmentResponse() { }
        /// <summary>
        /// Creates a success response.
        /// </summary>
        /// <param name="employee">Saved employee.</param>
        /// <returns>Response.</returns>
        public DepartmentResponse(DepartmentDto employee) : base(employee) { }

        /// <summary>
        /// Creates am error response.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <returns>Response.</returns>
        public DepartmentResponse(string message) : base(message) { }
    }
}
