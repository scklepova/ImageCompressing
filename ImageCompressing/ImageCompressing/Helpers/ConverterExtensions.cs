using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageCompressing.Helpers
{
    public static class ConverterExtensions
    {
        public static byte[] ToPixels(this ImageSource image)
        {
            var image1 = (BitmapSource)image;
            var stride = image1.PixelWidth * 4;
            var arraySize = image1.PixelHeight * stride;
            var pixels = new byte[arraySize];
            image1.CopyPixels(pixels, stride, 0);
            return pixels;
        }
    }
}