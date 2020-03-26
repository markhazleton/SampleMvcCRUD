namespace Mwh.SampleCRUD.BL.Models
{
    public class DeskBookingResult : DeskBookingBase
    {
        public DeskBookingResult()
        {
        }

        public DeskBookingResultCode Code { get; set; }
        public int? DeskBookingId { get; set; }
    }
}