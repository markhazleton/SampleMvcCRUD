// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-05-2020
//
// Last Modified By : mark
// Last Modified On : 04-05-2020
// ***********************************************************************
// <copyright file="EmployeeResponse.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
using Mwh.Sample.Common.Models;

namespace Mwh.Sample.Common.Repositories
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
