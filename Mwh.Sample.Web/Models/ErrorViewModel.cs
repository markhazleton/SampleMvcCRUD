namespace Mwh.Sample.Web.Models;
/// <summary>
/// View model for displaying error information to users
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the request identifier for error tracking purposes
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the request ID should be displayed to the user
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
