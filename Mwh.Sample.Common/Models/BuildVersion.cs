
namespace Mwh.Sample.Common.Models;

/// <summary>
/// Build Version
/// </summary>
public sealed class BuildVersion
{
    /// <summary>
    /// Build Version from assembly if null set all to 0
    /// </summary>
    /// <param name="assembly"></param>
    public BuildVersion(Assembly assembly)
    {
        Version? oVer = assembly?.GetName().Version;
        MajorVersion = oVer?.Major ?? 0;
        MinorVersion = oVer?.Minor ?? 0;
        Build = oVer?.Build ?? 0;
        Revision = oVer?.Revision ?? 0;
    }

    /// <summary>
    /// Override the To String Function to Format Version
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{MajorVersion}.{MinorVersion}.{Build}.{Revision}";
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
