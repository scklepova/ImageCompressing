using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImageCompressing.Components;

namespace ImageCompressing.Helpers
{
    public class WaveletTransformator
    {
        public static BitmapSource WaveletStep(Image image, int size, double threshold)
        {
            var source = ((BitmapSource) image.Source);
//            var ycbcr = MainWindow.ToYCbCr(source);
            var ycbcr = source.ToPixels();

            var compY = new byte[size*size];
            var compCb = new byte[size*size];
            var compCr = new byte[size*size];
            for (var i = 0; i < size*size*4; i += 4)
            {
                compY[i/4] = ycbcr[i];
                compCb[i/4] = ycbcr[i + 1];
                compCr[i/4] = ycbcr[i + 2];
            }
            var matrixY = JpegHelper.ToMatrix(compY.Select(x => (int)x).ToArray(), size);
            var matrixCb = JpegHelper.ToMatrix(compCb.Select(x => (int)x).ToArray(), size);
            var matrixCr = JpegHelper.ToMatrix(compCr.Select(x => (int)x).ToArray(), size);

            var d4 = GetDaubechiesMatrix(size);
            var inverseD4 = d4.GetTranspose(size);

            var transformedY = DaubechiesTransformation(matrixY, d4, size, threshold);
            var transformedCb = DaubechiesTransformation(matrixCb, d4, size, threshold);
            var transformedCr = DaubechiesTransformation(matrixCr, d4, size, threshold);

            //перемешать, записать в файл, сжать
//            var bytesCount = transformedY.Length * transformedY + transformedCr.Length + transformedCb.Length;

            var zerosCount = CountZeros(transformedY, size) + CountZeros(transformedCr, size) +
                             CountZeros(transformedCb, size);
            var totalCount = transformedY.Length*transformedY[0].Length*3;
            MessageBox.Show(string.Format("Zeros count = {0}{1} Total = {2}{1} Zeros percent = {3}", zerosCount, Environment.NewLine, totalCount, zerosCount * 100.0 / totalCount));

            var changedY = ChangeOrder(transformedY, size);
            var changedCb = ChangeOrder(transformedCb, size);
            var changedCr = ChangeOrder(transformedCr, size);
            WriteToFile(changedY, changedCb, changedCr, size);

            //back
            var backY = DaubechiesTransformation(transformedY, inverseD4, size, threshold, true);
            var backCb = DaubechiesTransformation(transformedCb, inverseD4, size, threshold, true);
            var backCr = DaubechiesTransformation(transformedCr, inverseD4, size, threshold, true);

            compY = JpegHelper.MatrixToPixels(backY, size).Select(x => MainWindow.GetByteValue(x)).ToArray();
            compCb = JpegHelper.MatrixToPixels(backCb, size).Select(x => MainWindow.GetByteValue(x)).ToArray();
            compCr = JpegHelper.MatrixToPixels(backCr, size).Select(x => MainWindow.GetByteValue(x)).ToArray();

            var pixels = new byte[size*size*4];
            for (var i = 0; i < size*size*4; i+=4)
            {
                pixels[i] = compY[i / 4];
                pixels[i + 1] = compCb[i / 4];
                pixels[i + 2] = compCr[i / 4];
            }

            return BitmapSource.Create(size, size, source.DpiX, source.DpiY, source.Format, null, pixels, source.PixelWidth * 4);
        }

        private static void WriteToFile(int[][] matrixY, int[][] matrixCb, int[][] matrixCr, int size)
        {
            var filename = "wavelet.wvlt";
            using (var writer = new BinaryWriter(File.Open(string.Format(filename), FileMode.Create)))
            {
                for(var i = 0; i < size; i++)
                    for (var j = 0; j < size; j++)
                    {
                        writer.Write(matrixY[i][j]);
                        writer.Write(matrixCb[i][j]);
                        writer.Write(matrixCr[i][j]);
                    }
                writer.Write(size);
            }
            JpegHelper.Zip7(filename);
        }

        private static int[][] ChangeOrder(int[][] matrix, int size)
        {
            var ans = new int[size][];
            //расставили строки
            for (var i = 0; i < size; i++)
            {
                var ind = i%2 == 0 ? i/2 : (i + size)/2;
                ans[ind] = matrix[i];
            }
            //столбцы
            var ans2 = new int[size][];
            for(var i = 0; i < size; i++)
                ans2[i] = new int[size];
            for (var j = 0; j < size; j++)
            {
                var ind = j % 2 == 0 ? j / 2 : (j + size) / 2;
                ans2.SetColumn(ans.GetColumn(size, j), size, ind);
            }
            return ans2;
        }

        private static int[][] DaubechiesTransformation(int[][] target, double[][] d4, int size, double threshold, bool isBack = false)
        {
            var doubleTarget = new double[size][];
            for (var i = 0; i < size; i++)
            {
                doubleTarget[i] = new double[size];
                for (var j = 0; j < size; j++)
                    doubleTarget[i][j] = target[i][j];
            }

            for (var i = 0; i < size; i++)
                doubleTarget[i] = d4.MultiplyBy(doubleTarget[i], size);

            for (var i = 0; i < size; i++)
                doubleTarget.SetColumn(d4.MultiplyBy(doubleTarget.GetColumn(size, i), size), size, i);

            //перемешивать надо по-хорошему еще
            var ans = new int[size][];
            for (var i = 0; i < size; i++)
            {
                ans[i] = new int[size];
                for (var j = 0; j < size; j++)
                {
                    if (!isBack && Math.Abs(doubleTarget[i][j]) < threshold)
                        ans[i][j] = 0;
                    else
                        ans[i][j] = (int) doubleTarget[i][j];
                }
            }

            return ans;
        }

        private static int CountZeros(int[][] matrix, int size)
        {
            var ans = 0;
            for (var i = 0; i < size; i++)
                ans += matrix[i].Count(x => x == 0);
            return ans;
        }

        private static double[][] GetDaubechiesMatrix(int size)
        {
            var ans = new double[size][];
            var k = 0;
            for (var i = 0; i < size; i++)
            {
                ans[i] = new double[size];
                if (i%2 == 0)
                {
                    ans[i][k] = c1;
                    ans[i][k + 1] = c2;
                    if (k + 2 == size)
                    {
                        ans[i][0] = c3;
                        ans[i][1] = c4;
                    }
                    else
                    {
                        ans[i][k + 2] = c3;
                        ans[i][k + 3] = c4;
                    }

                }
                else
                {
                    ans[i][k] = c4;
                    ans[i][k + 1] = -c3;
                    if (k + 2 == size)
                    {
                        ans[i][0] = c2;
                        ans[i][1] = -c1;
                    }
                    else
                    {
                        ans[i][k + 2] = c2;
                        ans[i][k + 3] = -c1;
                    }
                    k += 2;
                }                
            }
            return ans;
        }

        private static double c1 = (1 + Math.Sqrt(3))/4/Math.Sqrt(2);
        private static double c2 = (3 + Math.Sqrt(3))/4/Math.Sqrt(2);
        private static double c3 = (3 - Math.Sqrt(3))/4/Math.Sqrt(2);
        private static double c4 = (1 - Math.Sqrt(3))/4/Math.Sqrt(2);
    }
}