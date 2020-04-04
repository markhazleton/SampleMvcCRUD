using Mwh.Sample.Common.Models;

namespace Mwh.Sample.Common.Repositories
{
    public class EmployeeResponse : BaseResponse<EmployeeModel>
    {
        /// <summary>
        /// Creates a success response.
        /// </summary>
        /// <param name="employee">Saved employee.</param>
        /// <returns>Response.</returns>
        public EmployeeResponse(EmployeeModel employee) : base(employee)
        { }

        /// <summary>
        /// Creates am error response.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <returns>Response.</returns>
        public EmployeeResponse(string message) : base(message)
        { }
    }
}
