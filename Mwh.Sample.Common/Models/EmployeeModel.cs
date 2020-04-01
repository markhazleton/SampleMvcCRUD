using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mwh.Sample.Common.Models
{

    public static class EnumHelpers
    {
        /// <summary>
        /// Returns whether the given enum value is a defined value for its type.
        /// </summary>
        public static bool IsDefined<T>(this T enumValue)
            where T : Enum
            => EnumValueCache<T>.DefinedValues.Contains(enumValue);

        /// <summary>
        /// Caches the defined values for each enum type for which this class is accessed.
        /// </summary>
        private static class EnumValueCache<T>
            where T : Enum
        {
            public static readonly HashSet<T> DefinedValues = new HashSet<T>((T[])Enum.GetValues(typeof(T)));
        }
    }


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

        public bool IsValid 
        {
            get 
            {
                if (string.IsNullOrEmpty(Name)) return false;
                if (string.IsNullOrEmpty(State)) return false;
                if (string.IsNullOrEmpty(Country)) return false;
                if (!Department.IsDefined()) return false;
                if ((Age < 1)) return false;
                return true;
            }
        }
    }
}
