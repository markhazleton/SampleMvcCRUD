using Mwh.Sample.Common.Extension;
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

        public int Age { get; set; }

        public string Country { get; set; }

        public EmployeeDepartment Department { get; set; }

        [Key]
        public int EmployeeID { get; set; }

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return false;
                if (string.IsNullOrEmpty(State))
                    return false;
                if (string.IsNullOrEmpty(Country))
                    return false;
                if (!Department.IsDefined())
                    return false;
                if ((Age < 1))
                    return false;
                return true;
            }
        }

        public JobAssignmentCollection JobList { get; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string State { get; set; }
    }
}
