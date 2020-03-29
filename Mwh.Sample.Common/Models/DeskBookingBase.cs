using System;

namespace Mwh.Sample.Common.Models
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