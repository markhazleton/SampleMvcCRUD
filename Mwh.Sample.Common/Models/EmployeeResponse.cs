
namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Class EmployeeResponse.
    /// Implements the <see cref="Mwh.Sample.Common.Repositories.BaseResponse{Mwh.Sample.Common.Models.EmployeeModel}" />
    /// </summary>
    /// <seealso cref="Mwh.Sample.Common.Repositories.BaseResponse{Mwh.Sample.Common.Models.EmployeeModel}" />
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
