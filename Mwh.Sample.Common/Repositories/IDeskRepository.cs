// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-07-2020
//
// Last Modified By : mark
// Last Modified On : 04-07-2020
// ***********************************************************************
// <copyright file="IDeskRepository.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
using Mwh.Sample.Common.Models;
using System;
using System.Collections.Generic;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Interface IDeskRepository
    /// </summary>
    public interface IDeskRepository
    {
        /// <summary>
        /// Gets the available desks.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>IEnumerable&lt;Desk&gt;.</returns>
        IEnumerable<Desk> GetAvailableDesks(DateTime date);
    }
}
