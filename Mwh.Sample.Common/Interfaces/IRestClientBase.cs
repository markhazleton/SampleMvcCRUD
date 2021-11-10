namespace Mwh.Sample.Common.Clients;
/// <summary>
/// REST Client Base Interface
/// </summary>
public interface IRestClientBase
{
    /// <summary>
    /// Implement IDisposable
    /// </summary>
    void Dispose();

    /// <summary>
    /// Application Name added to header of all requests
    /// </summary>
    string AppName { get; set; }
    /// <summary>
    /// Base URL for all Requests
    /// </summary>
    string BaseAPIUrl { get; set; }
    /// <summary>
    /// Is Error
    /// </summary>
    bool IsError { get; set; }
    /// <summary>
    /// Status
    /// </summary>
    string Status { get; set; }
}
