using Mwh.Sample.Common.Models;

namespace Mwh.Sample.Common.Repositories
{
    public interface IDeskBookingRepository
    {
        void Save(DeskBooking deskBooking);
    }
}
