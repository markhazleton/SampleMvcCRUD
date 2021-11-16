
namespace Mwh.Sample.Domain.Models;

/// <summary>
/// Service Status Values1
/// </summary>
public enum ServiceStatus
{
    /// <summary>
    /// Some sub-set of the system is not working
    /// </summary>
    Degraded,
    /// <summary>
    /// The system is not available
    /// </summary>
    Offline,
    /// <summary>
    /// The system is fully online 
    /// </summary>
    Online
}
