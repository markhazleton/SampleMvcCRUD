
namespace Mwh.Sample.Domain.Tests.Extensions
{

    /// <summary>
    /// Enum TestEnum
    /// </summary>
    public enum TestEnum
    {
        /// <summary>
        /// The unknown
        /// </summary>
        [Display(Name = "Unknown", Description = "Unknown")]
        Unknown = 0,

        /// <summary>
        /// The first
        /// </summary>
        [Display(Name = "1st", Description = "The First One")]
        First = 1,

        /// <summary>
        /// The second
        /// </summary>
        Second = 2
    }
}
