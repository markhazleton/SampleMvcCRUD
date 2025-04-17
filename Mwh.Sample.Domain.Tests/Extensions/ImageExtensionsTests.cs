using System.Drawing;

namespace Mwh.Sample.Domain.Tests.Extensions;


[TestClass]
public class ImageExtensionsTests
{
    [TestMethod]
    public void Resize_WhenMaxWidthAndMaxHeightAreZero_ReturnsOriginalImage()
    {
        // Arrange
        Bitmap originalImage = new Bitmap(100, 200);

        // Act
        Image resizedImage = originalImage.Resize();

        // Assert
        Assert.AreEqual(originalImage.Width, resizedImage.Width);
        Assert.AreEqual(originalImage.Height, resizedImage.Height);
    }

    [TestMethod]
    public void Resize_WhenMaxWidthIsZero_ReturnsImageWithProportionalHeight()
    {
        // Arrange
        Bitmap originalImage = new Bitmap(100, 200);
        int maxHeight = 150;

        // Act
        Image resizedImage = originalImage.Resize(maxHeight: maxHeight);

        // Assert
        Assert.AreEqual(maxHeight, resizedImage.Height);
        Assert.IsTrue(resizedImage.Width <= originalImage.Width);
    }

    [TestMethod]
    public void Resize_WhenMaxHeightIsZero_ReturnsImageWithProportionalWidth()
    {
        // Arrange
        Bitmap originalImage = new Bitmap(500, 500);
        int maxWidth = 120;

        // Act
        Image resizedImage = originalImage.Resize(maxWidth: maxWidth);

        // Assert
        Assert.AreEqual(maxWidth, resizedImage.Width);
        Assert.IsTrue(resizedImage.Height <= originalImage.Height);
    }

    [TestMethod]
    public void ScaleImage_WhenMaxHeightIsGreaterThanOriginalHeight_ReturnsImageWithProportionalWidth()
    {
        // Arrange
        Bitmap originalImage = new Bitmap(100, 200);
        int maxHeight = 300;

        // Act
        Image scaledImage = originalImage.ScaleImage(maxHeight);

        // Assert
        Assert.AreEqual(maxHeight, scaledImage.Height);
        Assert.IsTrue(scaledImage.Width >= originalImage.Width);
    }
}
