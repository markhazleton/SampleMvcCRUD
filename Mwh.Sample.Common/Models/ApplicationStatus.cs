using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Mwh.Sample.Common.Models
{
    public class ApplicationStatus
    {
        Assembly _assembly;

        public ApplicationStatus(Assembly assembly)
        {
            _assembly = assembly;
            System.Version oVer = assembly?.GetName().Version;
            BuildVersion = new BuildVersion()
            {
                MajorVersion = oVer.Major,
                MinorVersion = oVer.Minor,
                Build = oVer.Build,
                Revision = oVer.Revision,
                BuildDate = GetBuildDate()
            };
            try
            {
                Tests.Add("Employee Database", "Success");
            }
            catch (Exception EE)
            {
                Tests.Add("Employee Database", "Failure");
                Messages.Add(EE.ToString());
            }

        }

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
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
            }
            return default;
        }

        public BuildVersion BuildVersion { get; set; }
        public Dictionary<string, string> Features { get; set; } = new Dictionary<string, string>();
        public List<string> Messages { get; set; } = new List<string>();
        public string Region { get; set; } = System.Environment.GetEnvironmentVariable("Region") ?? System.Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
        public ServiceStatus Status { get; set; } = ServiceStatus.Online;
        public Dictionary<string, string> Tests { get; set; } = new Dictionary<string, string>();
    }
}
