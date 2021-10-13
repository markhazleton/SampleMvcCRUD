using System;
using System.Reflection;

namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// Build Version
    /// </summary>
    public sealed class BuildVersion
    {
        public BuildVersion(Assembly assembly)
        {
            var oVer = assembly?.GetName().Version;
            MajorVersion = oVer.Major;
            MinorVersion = oVer.Minor;
            Build = oVer.Build;
            Revision = oVer.Revision;
        }

        public override string ToString()
        {
            return MajorVersion.ToString() + "." + MinorVersion.ToString() + "." + Build.ToString() + "." + Revision.ToString();
        }

        /// <summary>
        /// Build
        /// </summary>
        public int Build { get; }

        /// <summary>
        /// Major Version
        /// </summary>
        public int MajorVersion { get; }

        /// <summary>
        /// Minor Version
        /// </summary>
        public int MinorVersion { get; }

        /// <summary>
        /// Revision
        /// </summary>
        public int Revision { get; }
    }
}