using System;
using System.Collections.Generic;

namespace SampleCRUD.Models
{
    public class Employee
    {
        public Employee()
        {
            JobList = new List<JobAssignment>();
        }
        public int Age { get; set; }
        public string Country { get; set; }
        public int EmployeeID { get; set; }
        public List<JobAssignment> JobList { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
    }
}
