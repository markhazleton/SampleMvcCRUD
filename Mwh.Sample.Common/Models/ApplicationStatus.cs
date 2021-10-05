using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Mwh.Sample.Common.Models
{
    /// <summary>
    /// ApplicationStatus
    /// </summary>
    public class ApplicationStatus
    {
        private readonly Assembly _assembly;

        public DateTime BuildDate { get; set; }

        /// <summary>
        /// ApplicationStatus
        /// </summary>
        /// <param name="assembly"></param>
        public ApplicationStatus(Assembly assembly)
        {
            _assembly = assembly;
            BuildDate = GetBuildDate();
            Version oVer = assembly?.GetName().Version;
            BuildVersion = new BuildVersion()
            {
                MajorVersion = oVer.Major,
                MinorVersion = oVer.Minor,
                Build = oVer.Build,
                Revision = oVer.Revision
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Spellchecker", "CRRSP06:A misspelled word has been found", Justification = "<Pending>")]
        private DateTime GetBuildDate()
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// BuildVersion
        /// </summary>
        public BuildVersion BuildVersion { get; set; }
        /// <summary>
        /// Features
        /// </summary>
        public Dictionary<string, string> Features { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Messages
        /// </summary>
        public List<string> Messages { get; set; } = new List<string>();
        /// <summary>
        /// Region
        /// </summary>
        public string Region { get; set; } = System.Environment.GetEnvironmentVariable("Region") ?? System.Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
        /// <summary>
        /// Status 
        /// </summary>
        public ServiceStatus Status { get; set; } = ServiceStatus.Online;
        /// <summary>
        /// Tests 
        /// </summary>
        public Dictionary<string, string> Tests { get; set; } = new Dictionary<string, string>();
    }
}