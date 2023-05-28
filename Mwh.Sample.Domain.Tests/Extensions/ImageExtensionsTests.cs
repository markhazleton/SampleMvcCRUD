using System.Drawing;

namespace Mwh.Sample.Domain.Tests.Extensions;


[TestClass]
public class ImageExtensionsTests
{
    [TestMethod]
    public void Resize_WhenMaxWidthAndMaxHeightAreZero_ReturnsOriginalImage()
    {
        // Arrange
        var originalImage = new Bitmap(100, 200);

        // Act
        var resizedImage = originalImage.Resize();

        // Assert
        Assert.AreEqual(originalImage.Width, resizedImage.Width);
        Assert.AreEqual(originalImage.Height, resizedImage.Height);
    }

    [TestMethod]
    public void Resize_WhenMaxWidthIsZero_ReturnsImageWithProportionalHeight()
    {
        // Arrange
        var originalImage = new Bitmap(100, 200);
        var maxHeight = 150;

        // Act
        var resizedImage = originalImage.Resize(maxHeight: maxHeight);

        // Assert
        Assert.AreEqual(maxHeight, resizedImage.Height);
        Assert.IsTrue(resizedImage.Width <= originalImage.Width);
    }

    [TestMethod]
    public void Resize_WhenMaxHeightIsZero_ReturnsImageWithProportionalWidth()
    {
        // Arrange
        var originalImage = new Bitmap(500, 500);
        var maxWidth = 120;

        // Act
        var resizedImage = originalImage.Resize(maxWidth: maxWidth);

        // Assert
        Assert.AreEqual(maxWidth, resizedImage.Width);
        Assert.IsTrue(resizedImage.Height <= originalImage.Height);
    }

    [TestMethod]
    public void ScaleImage_WhenMaxHeightIsGreaterThanOriginalHeight_ReturnsImageWithProportionalWidth()
    {
        // Arrange
        var originalImage = new Bitmap(100, 200);
        var maxHeight = 300;

        // Act
        var scaledImage = originalImage.ScaleImage(maxHeight);

        // Assert
        Assert.AreEqual(maxHeight, scaledImage.Height);
        Assert.IsTrue(scaledImage.Width >= originalImage.Width);
    }
}
