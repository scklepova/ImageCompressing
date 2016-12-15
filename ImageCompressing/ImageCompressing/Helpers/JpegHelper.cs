using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ImageCompressing.Helpers
{
    public class JpegHelper
    {
        public static byte[] MenuItem_Downsampling(Image image1, int size, string type)
        {
            var image = (BitmapSource)image1.Source;
            var pixels = MainWindow.ToYCbCr(image);
            var matrix = ToMatrix(pixels, size);
            Action<int, int, int> downsampling12, downsampling22, downsampling21;
            downsampling12 = null;
            downsampling22 = null;
            downsampling21 = null;
            if (type == "2h1v")
            {
                downsampling12 = (i, j, k) => matrix[i][j + 4 + k] = matrix[i][j + k];
                downsampling22 = (i, j, k) => matrix[i + 1][j + 4 + k] = matrix[i + 1][j + k];
                downsampling21 = (i, j, k) => matrix[i + 1][j + k] = matrix[i + 1][j + k];
            }
            else if(type == "2h2v")
            {
                downsampling12 = (i, j, k) => matrix[i][j + 4 + k] = matrix[i][j + k];
                downsampling22 = (i, j, k) => matrix[i + 1][j + 4 + k] = matrix[i][j + k];
                downsampling21 = (i, j, k) => matrix[i + 1][j + k] = matrix[i][j + k];
            }
            else if (type == "1h2v")
            {
                downsampling12 = (i, j, k) => matrix[i][j + 4 + k] = matrix[i][j + 4 + k];
                downsampling22 = (i, j, k) => matrix[i + 1][j + 4 + k] = matrix[i][j + 4 + k];
                downsampling21 = (i, j, k) => matrix[i + 1][j + k] = matrix[i][j + k];
            }

            for (var i = 0; i < size; i += 2)
                for (var j = 0; j < size * 4; j += 8)
                {
                    if(downsampling12 != null) downsampling12(i, j, 1);
                    if(downsampling22 != null) downsampling12(i, j, 1);
                    if(downsampling21 != null) downsampling12(i, j, 1);

                    if (downsampling12 != null) downsampling12(i, j, 2);
                    if (downsampling22 != null) downsampling12(i, j, 2);
                    if (downsampling21 != null) downsampling12(i, j, 2);
                }
            pixels = MatrixToPixels(matrix, size);
            return pixels;
        }

        private static byte[][] ToMatrix(byte[] pixels, int size)
        {
            var i = -1;
            var matrix = new byte[size][];
            for (var k = 0; k < pixels.Length; k += 4)
            {
                var j = k % (size * 4);
                if (j == 0)
                {
                    i++;
                    matrix[i] = new byte[size * 4];
                }

                matrix[i][j] = pixels[k];
                matrix[i][j + 1] = pixels[k + 1];
                matrix[i][j + 2] = pixels[k + 2];
                matrix[i][j + 3] = pixels[k + 3];
            }

            return matrix;
        }

        private static byte[] MatrixToPixels(byte[][] matrix, int size)
        {
            var pixels = new byte[size * size * 4];
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size * 4; j++)
                    pixels[i * size * 4 + j] = matrix[i][j];
            return pixels;
        } 

    }
}