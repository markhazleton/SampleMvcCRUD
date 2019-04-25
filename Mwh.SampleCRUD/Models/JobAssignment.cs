using System;

namespace SampleCRUD.Models
{
    public class JobAssignment
    {
        public int CompLevel { get; set; }
        public DateTime EndDate { get; set; }
        public int OutcomeCode { get; set; }
        public DateTime StartDate { get; set; }
        public string Title { get; set; }

    }
}
