using SkiaSharp;

namespace Mwh.Sample.Domain.Extensions
{
    public static class ImageExtensions
    {
        public static SKBitmap Resize(this SKBitmap image, int maxWidth = 0, int maxHeight = 0)
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

            var resizedImage = image.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default);
            return resizedImage ?? image;
        }

        public static SKBitmap ScaleImage(this SKBitmap image, int maxHeight)
        {
            double ratio = (double)maxHeight / image.Height;

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            var scaledImage = image.Resize(new SKImageInfo(newWidth, newHeight), SKSamplingOptions.Default);
            return scaledImage ?? image;
        }
    }
}
