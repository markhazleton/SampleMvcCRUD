
using Mwh.Sample.Domain.Extensions;
using System.Drawing;
using System.Drawing.Imaging;

namespace Mwh.Sample.Web.Controllers;

/// <summary>
/// BaseController
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public abstract class BaseController : Controller
{
    private readonly IWebHostEnvironment webHostEnvironment;

    /// <summary>
    ///
    /// </summary>
    public readonly CancellationTokenSource cts;
    private readonly IConfiguration Config;

    /// <summary>
    /// BaseController
    /// </summary>
    protected BaseController(IConfiguration configuration,
        IWebHostEnvironment hostEnvironment)
    {
        cts = new CancellationTokenSource();
        this.Config = configuration;
        webHostEnvironment = hostEnvironment;
    }
    /// <summary>
    /// Update File and return File Name
    /// </summary>
    /// <param name="ProfileImage"></param>
    /// <returns></returns>
    protected string? UploadedFile(IFormFile? ProfileImage)
    {
        if (ProfileImage is null) return null;

        string filePath = Path.Combine(Path.Combine(webHostEnvironment.WebRootPath, "images"), $"{Guid.NewGuid()}_{$"{Path.GetFileNameWithoutExtension(ProfileImage.FileName)}.png"}");
        using Bitmap bmpPostedImage = new(ProfileImage.OpenReadStream());
        using Image objImage = bmpPostedImage.ScaleImage(81);
        objImage.Save(filePath, ImageFormat.Png);
        return Path.GetFileName(filePath);
    }

}
