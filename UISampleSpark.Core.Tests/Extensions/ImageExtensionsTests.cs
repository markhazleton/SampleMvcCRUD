using SkiaSharp;

namespace UISampleSpark.Core.Tests.Extensions;


[TestClass]
public class ImageExtensionsTests
{
    [TestMethod]
    public void Resize_WhenMaxWidthAndMaxHeightAreZero_ReturnsOriginalImage()
    {
        // Arrange
        using var originalImage = new SKBitmap(100, 200);

        // Act
        using var resizedImage = originalImage.Resize();

        // Assert
        Assert.AreEqual(originalImage.Width, resizedImage.Width);
        Assert.AreEqual(originalImage.Height, resizedImage.Height);
    }

    [TestMethod]
    public void Resize_WhenMaxWidthIsZero_ReturnsImageWithProportionalHeight()
    {
        // Arrange
        using var originalImage = new SKBitmap(100, 200);
        int maxHeight = 150;

        // Act
        using var resizedImage = originalImage.Resize(maxHeight: maxHeight);

        // Assert
        Assert.AreEqual(maxHeight, resizedImage.Height);
        Assert.IsTrue(resizedImage.Width <= originalImage.Width);
    }

    [TestMethod]
    public void Resize_WhenMaxHeightIsZero_ReturnsImageWithProportionalWidth()
    {
        // Arrange
        using var originalImage = new SKBitmap(500, 500);
        int maxWidth = 120;

        // Act
        using var resizedImage = originalImage.Resize(maxWidth: maxWidth);

        // Assert
        Assert.AreEqual(maxWidth, resizedImage.Width);
        Assert.IsTrue(resizedImage.Height <= originalImage.Height);
    }

    [TestMethod]
    public void ScaleImage_WhenMaxHeightIsGreaterThanOriginalHeight_ReturnsImageWithProportionalWidth()
    {
        // Arrange
        using var originalImage = new SKBitmap(100, 200);
        int maxHeight = 300;

        // Act
        using var scaledImage = originalImage.ScaleImage(maxHeight);

        // Assert
        Assert.AreEqual(maxHeight, scaledImage.Height);
        Assert.IsTrue(scaledImage.Width >= originalImage.Width);
    }
}
