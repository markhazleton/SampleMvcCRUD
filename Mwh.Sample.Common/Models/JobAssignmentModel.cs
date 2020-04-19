// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-01-2020
//
// Last Modified By : mark
// Last Modified On : 04-01-2020
// ***********************************************************************
// <copyright file="JobAssignmentModel.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Job Assignment
    /// </summary>
    public class JobAssignmentModel
    {
        /// <summary>
        /// Gets or sets the comp level.
        /// </summary>
        /// <value>The comp level.</value>
        public int CompLevel { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>The end date.</value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the outcome code.
        /// </summary>
        /// <value>The outcome code.</value>
        public int OutcomeCode { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }
    }
}
