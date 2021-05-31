using System;

namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Build Version
    /// </summary>
    public class BuildVersion
    {
        public override string ToString()
        {
            return MajorVersion.ToString() + "." + MinorVersion.ToString() + "." + Build.ToString() + "." + Revision.ToString();
        }

        /// <summary>
        /// Build
        /// </summary>
        public int Build { get; set; }
        /// <summary>
        /// Date of Build
        /// </summary>
        public DateTime BuildDate { get; set; }
        /// <summary>
        /// Major Version
        /// </summary>
        public int MajorVersion { get; set; }
        /// <summary>
        /// Minor Version
        /// </summary>
        public int MinorVersion { get; set; }
        /// <summary>
        /// Revision
        /// </summary>
        public int Revision { get; set; }
    }
}
