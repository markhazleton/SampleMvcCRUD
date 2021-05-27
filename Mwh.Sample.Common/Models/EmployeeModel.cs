using Mwh.Sample.Common.Extension;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>The age.</value>
        [JsonPropertyName("age")]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>The country.</value>
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the department.
        /// </summary>
        /// <value>The department.</value>
        [JsonPropertyName("department")]
        public EmployeeDepartment Department { get; set; }

        /// <summary>
        /// Gets or sets the employee identifier.
        /// </summary>
        /// <value>The employee identifier.</value>
        [JsonPropertyName("employee_id")]
        public int EmployeeID { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets the job list.
        /// </summary>
        /// <value>The job list.</value>
        [JsonPropertyName("job_list")]
        public JobAssignmentCollection JobList { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        [Required]
        [JsonPropertyName("state")]
        public string State { get; set; }
    }
}
