using System;

namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Job Assignment
    /// </summary>
    public class JobAssignmentModel
    {
        public int CompLevel { get; set; }

        public DateTime EndDate { get; set; }

        public int OutcomeCode { get; set; }

        public DateTime StartDate { get; set; }

        public string Title { get; set; }
    }
}
