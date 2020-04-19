// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-01-2020
//
// Last Modified By : mark
// Last Modified On : 04-01-2020
// ***********************************************************************
// <copyright file="DeskBookingBase.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Class DeskBookingBase.
    /// </summary>
    public abstract class DeskBookingBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeskBookingBase"/> class.
        /// </summary>
        public DeskBookingBase()
        {
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }
        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public DateTime Date { get; set; }
    }
}