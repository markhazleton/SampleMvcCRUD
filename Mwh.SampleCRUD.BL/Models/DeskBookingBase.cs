using System;

namespace Mwh.SampleCRUD.BL.Models
{
    public abstract class DeskBookingBase
    {
        public DeskBookingBase()
        {
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Date { get; set; }
    }
}