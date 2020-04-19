// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-01-2020
//
// Last Modified By : mark
// Last Modified On : 04-01-2020
// ***********************************************************************
// <copyright file="DeskBooking.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Class DeskBooking.
    /// Implements the <see cref="Mwh.Sample.Common.Models.DeskBookingBase" />
    /// </summary>
    /// <seealso cref="Mwh.Sample.Common.Models.DeskBookingBase" />
    public class DeskBooking : DeskBookingBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeskBooking"/> class.
        /// </summary>
        public DeskBooking() { }

        /// <summary>
        /// Gets or sets the desk identifier.
        /// </summary>
        /// <value>The desk identifier.</value>
        public int DeskId { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }
    }
}

