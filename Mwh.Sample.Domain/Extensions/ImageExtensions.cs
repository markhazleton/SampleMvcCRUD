using System.Drawing;

namespace Mwh.Sample.Domain.Extensions
{
    public static class ImageExtensions
    {
        public static System.Drawing.Image Resize(this System.Drawing.Image image, int maxWidth = 0, int maxHeight = 0)
        {
            if (maxWidth == 0)
                maxWidth = image.Width;
            if (maxHeight == 0)
                maxHeight = image.Height;

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }
        public static System.Drawing.Image ScaleImage(this System.Drawing.Image image, int maxHeight)
        {
            var ratio = (double)maxHeight / image.Height;

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
    }
}
