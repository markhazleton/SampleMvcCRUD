
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Mwh.Sample.Core.WebApi.Controllers
{
    /// <summary>
    /// Base for all Api Controllers in this project
    /// </summary>
    [Produces("application/json")]
    [ApiController]
    public abstract class BaseApiController : Controller
    {

        private static DateTime GetBuildDate(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
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


        protected BuildVersion GetBuildVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            System.Version oVer = assembly?.GetName().Version;
            return new BuildVersion()
            {
                MajorVersion = oVer.Major,
                MinorVersion = oVer.Minor,
                Build = oVer.Build,
                Revision = oVer.Revision,
                BuildDate = GetBuildDate(assembly)
            };
        }
        /// <summary>
        /// GetApplicationStatus
        /// </summary>
        /// <returns></returns>
        protected ApplicationStatus GetApplicationStatus()
        {
            var status = new ApplicationStatus()
            {
                BuildVersion = GetBuildVersion()
            };

            try
            {
                status.Tests.Add("Employee Database", "Success");
            }
            catch (Exception EE)
            {
                status.Tests.Add("Employee Database", "Failure");
                status.Messages.Add(EE.ToString());
            }
            return status;
        }

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

        public enum ServiceStatus
        { 
            Degraded,
            Offline,
            Online
        }

        public class ApplicationStatus
        {
            public BuildVersion BuildVersion { get; set; }
            public Dictionary<string, string> Features { get; set; } = new Dictionary<string, string>();
            public List<string> Messages { get; set; } = new List<string>();
            public string Region { get; set; } = System.Environment.GetEnvironmentVariable("Region") ?? System.Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
            public ServiceStatus Status { get; set; } = ServiceStatus.Online;
            public Dictionary<string, string> Tests { get; set; } = new Dictionary<string, string>();

        }

    }

}
