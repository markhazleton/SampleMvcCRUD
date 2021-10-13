using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Mwh.Sample.Common.Models
{
    [SuppressMessage("Spellchecker", "CRRSP06:A misspelled word has been found", Justification = "<Pending>")]

    /// <summary>
    /// ApplicationStatus
    /// </summary>
    public sealed class ApplicationStatus
    {
        /// <summary>
        /// ApplicationStatus
        /// </summary>
        /// <param name="assembly"></param>
        public ApplicationStatus(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        BuildDate = result;
                    }
                }
            }
            BuildVersion = new BuildVersion(assembly);
        }

        public DateTime BuildDate { get; }
        /// <summary>
        /// BuildVersion
        /// </summary>
        public BuildVersion BuildVersion { get; }
        /// <summary>
        /// Features
        /// </summary>
        public Dictionary<string, string> Features { get; } = new Dictionary<string, string>();
        /// <summary>
        /// Messages
        /// </summary>
        public List<string> Messages { get; } = new List<string>();
        /// <summary>
        /// Region
        /// </summary>
        public string Region { get; } = Environment.GetEnvironmentVariable("Region") ?? Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
        /// <summary>
        /// Status 
        /// </summary>
        public ServiceStatus Status { get; } = ServiceStatus.Online;
        /// <summary>
        /// Tests 
        /// </summary>
        public Dictionary<string, string> Tests { get; } = new Dictionary<string, string>();
    }
}