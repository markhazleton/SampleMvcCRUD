
using Mwh.Sample.Domain.Extensions;
using SkiaSharp;

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
    /// <param name="EmployeeId"></param>
    protected string? UploadedFile(IFormFile? ProfileImage, string EmployeeId)
    {
        if (ProfileImage is null) return null;
        string folderPath = Path.Combine(webHostEnvironment.WebRootPath, "images", EmployeeId);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string filePath = Path.Combine(folderPath, $"{Guid.NewGuid()}_{$"{Path.GetFileNameWithoutExtension(ProfileImage.FileName)}.png"}");

        using var stream = ProfileImage.OpenReadStream();
        using var original = SKBitmap.Decode(stream);
        using var scaled = original.ScaleImage(81);
        using var image = SKImage.FromBitmap(scaled);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var fileStream = System.IO.File.OpenWrite(filePath);
        data.SaveTo(fileStream);

        return $"{EmployeeId}/{Path.GetFileName(filePath)}";
    }


}
