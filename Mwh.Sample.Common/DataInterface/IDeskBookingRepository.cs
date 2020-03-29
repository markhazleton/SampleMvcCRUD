using Mwh.Sample.Common.Models;

namespace Mwh.Sample.Common.DataInterface
{
    public interface IDeskBookingRepository
    {
        void Save(DeskBooking deskBooking);
    }
}
