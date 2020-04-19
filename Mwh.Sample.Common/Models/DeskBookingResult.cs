// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-01-2020
//
// Last Modified By : mark
// Last Modified On : 04-01-2020
// ***********************************************************************
// <copyright file="DeskBookingResult.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Class DeskBookingResult.
    /// Implements the <see cref="Mwh.Sample.Common.Models.DeskBookingBase" />
    /// </summary>
    /// <seealso cref="Mwh.Sample.Common.Models.DeskBookingBase" />
    public class DeskBookingResult : DeskBookingBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeskBookingResult"/> class.
        /// </summary>
        public DeskBookingResult()
        {
        }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>The code.</value>
        public DeskBookingResultCode Code { get; set; }
        /// <summary>
        /// Gets or sets the desk booking identifier.
        /// </summary>
        /// <value>The desk booking identifier.</value>
        public int? DeskBookingId { get; set; }
    }
}