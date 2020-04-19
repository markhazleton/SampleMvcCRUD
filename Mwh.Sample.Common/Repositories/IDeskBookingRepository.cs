// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-07-2020
//
// Last Modified By : mark
// Last Modified On : 04-07-2020
// ***********************************************************************
// <copyright file="IDeskBookingRepository.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
using Mwh.Sample.Common.Models;

namespace Mwh.Sample.Common.Repositories
{
    /// <summary>
    /// Interface IDeskBookingRepository
    /// </summary>
    public interface IDeskBookingRepository
    {
        /// <summary>
        /// Saves the specified desk booking.
        /// </summary>
        /// <param name="deskBooking">The desk booking.</param>
        void Save(DeskBooking deskBooking);
    }
}
