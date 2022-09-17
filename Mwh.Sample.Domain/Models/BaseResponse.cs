
namespace Mwh.Sample.Domain.Models;

/// <summary>
/// Class BaseResponse.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseResponse<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
    /// </summary>
    /// <param name="resource">The resource.</param>
    protected BaseResponse(T? resource)
    {
        Success = resource != null;
        Message = resource == null ? "Resource is null" : string.Empty;
        Resource = resource;
    }
    protected BaseResponse(bool IsSucces)
    {
        Success = IsSucces;
        Message = String.Empty;
        Resource = default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    protected BaseResponse(string message)
    {
        Success = false;
        Message = message;
        Resource = default;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseResponse{T}"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    protected BaseResponse()
    {
        Success = false;
        Message = "Empty Initialize";
        Resource = default;
    }

    /// <summary>
    /// Gets the message.
    /// </summary>
    /// <value>The message.</value>
    public string Message { get; set; }

    /// <summary>
    /// Gets the resource.
    /// </summary>
    /// <value>The resource.</value>
    public T? Resource { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="BaseResponse{T}"/> is success.
    /// </summary>
    /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
    public bool Success { get; set; }
}
