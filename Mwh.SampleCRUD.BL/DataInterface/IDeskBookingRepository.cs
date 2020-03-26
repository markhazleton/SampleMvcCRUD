using Mwh.SampleCRUD.BL.Models;

namespace Mwh.SampleCRUD.BL.DataInterface
{
    public interface IDeskBookingRepository
    {
        void Save(DeskBooking deskBooking);
    }
}
