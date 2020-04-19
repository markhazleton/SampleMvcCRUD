// ***********************************************************************
// Assembly         : Mwh.Sample.Common
// Author           : mark
// Created          : 04-01-2020
//
// Last Modified By : mark
// Last Modified On : 04-07-2020
// ***********************************************************************
// <copyright file="DeskBookingRequestProcessor.cs" company="Mark Hazleton">
//     Copyright 2020 Mark Hazleton
// </copyright>
// <summary></summary>
// ***********************************************************************
using Mwh.Sample.Common.Models;
using Mwh.Sample.Common.Repositories;
using System;
using System.Linq;

namespace Mwh.Sample.Common.Processor
{
    /// <summary>
    /// DeskBookingRequestProcessor
    /// </summary>
    public class DeskBookingRequestProcessor
    {
        /// <summary>
        /// The desk booking repository
        /// </summary>
        private readonly IDeskBookingRepository _deskBookingRepository;
        /// <summary>
        /// The desk repository
        /// </summary>
        private readonly IDeskRepository _deskRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeskBookingRequestProcessor"/> class.
        /// </summary>
        /// <param name="deskBookingRepository">The desk booking repository.</param>
        /// <param name="deskRepository">The desk repository.</param>
        public DeskBookingRequestProcessor(IDeskBookingRepository deskBookingRepository, IDeskRepository deskRepository)
        {
            _deskBookingRepository = deskBookingRepository;
            _deskRepository = deskRepository;
        }

        /// <summary>
        /// Books the desk.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>DeskBookingResult.</returns>
        /// <exception cref="ArgumentNullException">request</exception>
        public DeskBookingResult BookDesk(DeskBookingRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var result = Create<DeskBookingResult>(request);

            var availableDesks = _deskRepository.GetAvailableDesks(request.Date);
            if (availableDesks.FirstOrDefault() is Desk availableDesk)
            {
                var deskBooking = Create<DeskBooking>(request);
                deskBooking.DeskId = availableDesk.Id;

                _deskBookingRepository.Save(deskBooking);

                result.DeskBookingId = deskBooking.Id;
                result.Code = DeskBookingResultCode.Success;
            }
            else
            {
                result.Code = DeskBookingResultCode.NoDeskAvailable;
            }

            return result;

        }

        /// <summary>
        /// Creates the specified request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns>T.</returns>
        private static T Create<T>(DeskBookingRequest request) where T : DeskBookingBase, new()
        {
            return new T
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Date = request.Date
            };
        }
    }
}