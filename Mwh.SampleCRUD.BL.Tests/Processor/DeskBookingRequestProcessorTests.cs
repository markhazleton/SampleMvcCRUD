using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mwh.SampleCRUD.BL.DataInterface;
using Mwh.SampleCRUD.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mwh.SampleCRUD.BL.Processor
{
    /// <summary>
    /// DeskBookingRequestProcessorTests
    /// </summary>
    [TestClass]
    public class DeskBookingRequestProcessorTests
    {
        private Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        private Mock<IDeskRepository> _deskRepositoryMock;
        private List<Desk> _availableDesks;
        private DeskBookingRequestProcessor _processor;
        private DeskBookingRequest _request;

        [TestMethod]
        public void ShouldReturnDeskBookingResultWithRequestValues()
        {
            // Arrange


            // Act
            DeskBookingResult result = _processor.BookDesk(_request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.FirstName, _request.FirstName);
            Assert.AreEqual(result.LastName, _request.LastName);
            Assert.AreEqual(result.Email, _request.Email);
            Assert.AreEqual(result.Date, _request.Date);
        }

        [TestMethod]
        public void ShouldSaveDeskBooking()
        {
            DeskBooking savedDeskBooking = null;
            _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                .Callback<DeskBooking>(deskBooking =>
                {
                    savedDeskBooking = deskBooking;
                });
            _processor.BookDesk(_request);

            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Once);

            Assert.IsNotNull(savedDeskBooking);
            Assert.AreEqual(_request.FirstName, savedDeskBooking.FirstName);
            Assert.AreEqual(_request.LastName, savedDeskBooking.LastName);
            Assert.AreEqual(_request.Email, savedDeskBooking.Email);
            Assert.AreEqual(_request.Date, savedDeskBooking.Date);
            Assert.AreEqual(_availableDesks.First().Id, savedDeskBooking.DeskId);
        }

        [TestMethod]
        public void ShouldNotSaveDeskBookingIfNoDeskIsAvailable()
        {
            _availableDesks.Clear();

            _processor.BookDesk(_request);

            _deskBookingRepositoryMock.Verify(x => x.Save(It.IsAny<DeskBooking>()), Times.Never);

        }


        [DataTestMethod]
        [DataRow(DeskBookingResultCode.Success, true)]
        [DataRow(DeskBookingResultCode.NoDeskAvailable, false)]
        public void ShouldReturnExpectedResultCode(
          DeskBookingResultCode expectedResultCode, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }

            var result = _processor.BookDesk(_request);

            Assert.AreEqual(expectedResultCode, result.Code);
        }

        [DataTestMethod]
        [DataRow(5, true)]
        [DataRow(null, false)]
        public void ShouldReturnExpectedDeskBookingId(
          int? expectedDeskBookingId, bool isDeskAvailable)
        {
            if (!isDeskAvailable)
            {
                _availableDesks.Clear();
            }
            else
            {
                _deskBookingRepositoryMock.Setup(x => x.Save(It.IsAny<DeskBooking>()))
                  .Callback<DeskBooking>(deskBooking =>
                  {
                      deskBooking.Id = expectedDeskBookingId.Value;
                  });
            }

            var result = _processor.BookDesk(_request);

            Assert.AreEqual(expectedDeskBookingId, result.DeskBookingId);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfRequestIsNull() { _processor.BookDesk(null); }

        [TestInitialize]
        public void TestInitialize()
        {
            _request = new DeskBookingRequest()
            {
                FirstName = "Mark",
                LastName = "Hazleton",
                Email = "mark.hazleton@gmail.com",
                Date = new DateTime(2020, 04, 15)
            };
            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();

            _availableDesks = new List<Desk> { new Desk { Id = 7 } };

            _deskRepositoryMock = new Mock<IDeskRepository>();

            _deskRepositoryMock.Setup(x => x.GetAvailableDesks(_request.Date)).Returns(_availableDesks);

            _processor = new DeskBookingRequestProcessor(_deskBookingRepositoryMock.Object, _deskRepositoryMock.Object);

        }
    }
}
