using System;

namespace Mwh.SampleCRUD.BL.Models
{
    /// <summary>
    /// Job Assignment
    /// </summary>
    public class JobAssignment
    {
        public int CompLevel { get; set; }

        public DateTime EndDate { get; set; }

        public int OutcomeCode { get; set; }

        public DateTime StartDate { get; set; }

        public string Title { get; set; }
    }
}
