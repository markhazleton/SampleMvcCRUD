using System.ComponentModel.DataAnnotations;

namespace Mwh.Sample.Common.Models
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
            JobList = new JobAssignmentCollection();
            Department = EmployeeDepartment.Unknown;
        }

        [Key]
        public int EmployeeID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string State { get; set; }

        public EmployeeDepartment Department { get; set; }

        public int Age { get; set; }

        public string Country { get; set; }

        public JobAssignmentCollection JobList { get; }
    }
}
