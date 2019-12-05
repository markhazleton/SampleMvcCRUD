using System.ComponentModel.DataAnnotations;

namespace Mwh.SampleCRUD.BL.Models
{
    /// <summary>
    /// Employee Model
    /// </summary>
    public class EmployeeModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EmployeeModel()
        {
            JobList = new JobAssignmentList();
            Department = EmployeeDepartment.Unknown;
        }

        public EmployeeDepartment Department { get; set; }

        public int Age { get; set; }

        public string Country { get; set; }

        public int EmployeeID { get; set; }

        public JobAssignmentList JobList { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string State { get; set; }
    }
}
