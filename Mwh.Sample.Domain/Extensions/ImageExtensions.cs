using System.Drawing;

namespace Mwh.Sample.Domain.Extensions
{
    public static class ImageExtensions
    {
        public static Image Resize(this Image image, int maxWidth = 0, int maxHeight = 0)
        {
            if (maxWidth == 0)
                maxWidth = image.Width;
            if (maxHeight == 0)
                maxHeight = image.Height;

            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }
        public static Image ScaleImage(this Image image, int maxHeight)
        {
            double ratio = (double)maxHeight / image.Height;

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
    }
}
