
namespace Mwh.Sample.Common.Models;

/// <summary>
/// Build Version
/// </summary>
public sealed class BuildVersion
{
    /// <summary>
    /// Build Version
    /// </summary>
    /// <param name="assembly"></param>
    public BuildVersion(Assembly assembly)
    {
        var oVer = assembly?.GetName().Version;
        MajorVersion = oVer.Major;
        MinorVersion = oVer.Minor;
        Build = oVer.Build;
        Revision = oVer.Revision;
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
