using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImageCompressing.Components;
using MoreLinq;

namespace ImageCompressing.Helpers
{
    public class JpegHelper
    {
        public static BitmapSource JpegSteps(Image image, int size, string downsamplingType, QuantizingType quantizingType, 
            int alphaY, int gammaY, int alphaC, int gammaC, int remainingCountY, int remainingCountC)
        {
            var source = ((BitmapSource)image.Source);
            var ycbcr = MainWindow.ToYCbCr(source);

            var compY = new byte[size * size];
            var compCb = new byte[size * size];
            var compCr = new byte[size * size];
            for (var i = 0; i < size*size*4; i += 4)
            {
                compY[i/4] = ycbcr[i];
                compCb[i/4] = ycbcr[i + 1];
                compCr[i/4] = ycbcr[i + 2];
            }

            //*** Downsampling
            var matrixY = ToMatrix(compY, size);
            var matrixCb = Downsampling(ToMatrix(compCb, size), size, downsamplingType);
            var matrixCr = Downsampling(ToMatrix(compCr, size), size, downsamplingType);

            //*** Splitting to blocks 8x8 and DCT
            var dct = new DiscreteCosineTransformator();
            var hSize = size/getHorSizeLossByType[downsamplingType];
            var vSize = size/getVertSizeLossByType[downsamplingType];
            var yBlocks = SplitTo8x8Matrices(matrixY, size, size).Select(dct.DCT).ToList();
            var cbBlocks = SplitTo8x8Matrices(matrixCb, hSize, vSize).Select(dct.DCT).ToList();
            var crBlocks = SplitTo8x8Matrices(matrixCr, hSize, vSize).Select(dct.DCT).ToList(); 

            //*** Quantizing
            var quantizorY = new JpegQuantisor(alphaY, gammaY);
            var quantizorC = new JpegQuantisor(alphaC, gammaC);
            switch (quantizingType)
            {
                case QuantizingType.Nullify:
                    yBlocks = yBlocks.Select(x => quantizorY.SimpleQuantizing(x, remainingCountY)).ToList();
                    cbBlocks = cbBlocks.Select(x => quantizorC.SimpleQuantizing(x, remainingCountC)).ToList();
                    crBlocks = crBlocks.Select(x => quantizorC.SimpleQuantizing(x, remainingCountC)).ToList();
                    break;
                case QuantizingType.AlphaGamma:
                    yBlocks = yBlocks.Select(quantizorY.QQuantizing).ToList();
                    cbBlocks = cbBlocks.Select(quantizorC.QQuantizing).ToList();
                    crBlocks = crBlocks.Select(quantizorC.QQuantizing).ToList();
                    break;
                default:
                    yBlocks = yBlocks.Select(x => quantizorY.QuantizingRecommended(x, "Y")).ToList();
                    cbBlocks = cbBlocks.Select(x => quantizorC.QuantizingRecommended(x, "Cb")).ToList();
                    crBlocks = crBlocks.Select(x => quantizorC.QuantizingRecommended(x, "Cr")).ToList();
                    break;
            }

            //*** saving to file
            
            WriteToFile(size, size, yBlocks, cbBlocks, crBlocks, downsamplingType, quantizingType, alphaY, gammaY, alphaC,gammaC, remainingCountY, remainingCountC);

            //*** back steps:

            //*** back quantizing
            switch (quantizingType)
            {
                case QuantizingType.Nullify:
                    break;
                case QuantizingType.AlphaGamma:
                    yBlocks = yBlocks.Select(quantizorY.BackQQuantizing).ToList();
                    cbBlocks = cbBlocks.Select(quantizorC.BackQQuantizing).ToList();
                    crBlocks = crBlocks.Select(quantizorC.BackQQuantizing).ToList();
                    break;
                default:
                    yBlocks = yBlocks.Select(x => quantizorY.BackQuantizingRecommended(x, "Y")).ToList();
                    cbBlocks = cbBlocks.Select(x => quantizorC.BackQuantizingRecommended(x, "Cb")).ToList();
                    crBlocks = crBlocks.Select(x => quantizorC.BackQuantizingRecommended(x, "Cr")).ToList();
                    break;
            }

            //*** back DCT
            yBlocks = yBlocks.Select(dct.InverseDCT).ToList();
            cbBlocks = cbBlocks.Select(dct.InverseDCT).ToList();
            crBlocks = crBlocks.Select(dct.InverseDCT).ToList();

            //*** back to whole matrix

            var backY = Matrices8x8ToOne(yBlocks.ToList(), size, size);
            var backCb = Matrices8x8ToOne(cbBlocks.ToList(), hSize, vSize);
            var backCr = Matrices8x8ToOne(crBlocks.ToList(), hSize, vSize);

            //*** back downsampling
            backCb = BackDownsampling(backCb, size, downsamplingType);
            backCr = BackDownsampling(backCr, size, downsamplingType);

            compY = MatrixToPixels(backY, size);
            compCb = MatrixToPixels(backCb, size);
            compCr = MatrixToPixels(backCr, size);

            var pixels = new byte[size*size*4];
            for (var i = 0; i < size*size*4; i+=4)
            {
                pixels[i] = compY[i / 4];
                pixels[i + 1] = compCb[i / 4];
                pixels[i + 2] = compCr[i / 4];
            }

            return MainWindow.ToRGB(pixels, source);
        }

        public static byte[][] Downsampling(byte[][] source, int size, string type)
        {
            if (type == "1h1v")
                return source;
            var ans = new byte[size/getVertSizeLossByType[type]][];
            var k = 0;
            var t = 0;
            var vertStep = getVertSizeLossByType[type];
            var horStep = getHorSizeLossByType[type];

            for (var i = 0; i < size; i += vertStep)
            {
                ans[k] = new byte[size / horStep];
                t = 0;
                for (var j = 0; j < size; j += horStep)
                {
                    ans[k][t] = source[i][j];
                    t++;
                }
                k++;
            }

            return ans;
        }

        public static byte[][] BackDownsampling(byte[][] source, int targetSize, string type)
        {
            if (type == "1h1v")
                return source;
            Action<int, int> downsampling12 = null;
            Action<int, int> downsampling22 = null;
            Action<int, int> downsampling21 = null;
            var matrix = new byte[targetSize][];
            int xStep = 0, yStep = 0;
            switch (type)
            {
                case "2h1v":
                    downsampling12 = (i, j) => matrix[i][j + 1] = matrix[i][j];
                    downsampling22 = (i, j) => matrix[i + 1][j + 1] = matrix[i + 1][j];
                    xStep = 2;
                    yStep = 1;
                    break;
                case "2h2v":
                    downsampling12 = (i, j) => matrix[i][j + 1] = matrix[i][j];
                    downsampling22 = (i, j) => matrix[i + 1][j + 1] = matrix[i][j];
                    downsampling21 = (i, j) => matrix[i + 1][j] = matrix[i][j];
                    xStep = 2;
                    yStep = 2;
                    break;
                case "1h2v":
                    downsampling22 = (i, j) => matrix[i + 1][j + 1] = matrix[i][j + 1];
                    downsampling21 = (i, j) => matrix[i + 1][j] = matrix[i][j];
                    xStep = 1;
                    yStep = 2;
                    break;
            }

            for (var i = 0; i < targetSize/yStep; i++)
            {
                matrix[i*yStep] = new byte[targetSize];
                if( yStep > 1) matrix[i*yStep + 1] = new byte[targetSize];
                for (var j = 0; j < targetSize/xStep; j++)
                    matrix[i*yStep][j*xStep] = source[i][j];
            }

            for (var i = 0; i < targetSize; i += 2)
                for (var j = 0; j < targetSize; j += 2)
                {
                    if(downsampling12 != null) downsampling12(i, j);
                    if(downsampling22 != null) downsampling22(i, j);
                    if(downsampling21 != null) downsampling21(i, j);
                }
            return matrix;
        }

        private static IEnumerable<byte[][]> SplitTo8x8Matrices(byte[][] source, int hSize, int vSize)
        {
            var ans = new List<byte[][]>();
            for(var i = 0; i < vSize; i+=8)
                for (var j = 0; j < hSize; j += 8)
                {
                    var m = new byte[8][];
                    for (var u = 0; u < 8; u++)
                    {
                        m[u] = new byte[8];
                        for (var v = 0; v < 8; v++)
                            m[u][v] = source[u + i][v + j];
                    }
                    ans.Add(m);
                }
            return ans;
        }

        private static byte[][] Matrices8x8ToOne(List<int[][]> source, int hSize, int vSize)
        {
            var ans = new byte[vSize][];
            var k = 0;
            for (var i = 0; i < vSize; i += 8)
                for (var j = 0; j < hSize; j += 8)
                {
                    for (var u = 0; u < 8; u++)
                    {
                        if(ans[u + i] == null )ans[u + i] = new byte[hSize];
                        for (var v = 0; v < 8; v++)
                            ans[u + i][v + j] = MainWindow.GetByteValue(source[k][u][v]);
                    }
                    k++;
                }
            return ans;
        }

        public static T[][] ToMatrix<T>(T[] pixels, int size)
        {
            var i = -1;
            var matrix = new T[size][];
            for (var k = 0; k < pixels.Length; k ++)
            {
                var j = k % size;
                if (j == 0)
                {
                    i++;
                    matrix[i] = new T[size];
                }

                matrix[i][j] = pixels[k];
            }

            return matrix;
        }

        public static T[] MatrixToPixels<T>(T[][] matrix, int size)
        {
            var pixels = new T[size * size];
            for (var i = 0; i < size; i++)
                for (var j = 0; j < size; j++)
                    pixels[i * size + j] = matrix[i][j];
            return pixels;
        }

        private static void WriteToFile(int width, int height, IEnumerable<int[][]> yBlocks, IEnumerable<int[][]> cbBlocks, IEnumerable<int[][]> crBlocks,
            string subsamplingType, QuantizingType quantizingType, int alphaY, int gammaY, int alphaC, int gammaC, int nY, int nC)
        {
            //*** saving to file
            // format: height, width, number of yblocks, yblocks, number of cbBlocks, cbBlocks, number of crBlocks, crBlocks,
            //         subsampling type, 
            //         quantizing type
            //         Ny Nc
            //         alpha gamma
            var filename = "test.myjpg";
            using (var writer = new BinaryWriter(File.Open(string.Format(filename), FileMode.Create)))
            {
                writer.Write(height);
                writer.Write(width);
                writer.Write(yBlocks.Count());
                foreach (var block in yBlocks.Select(x => x.ToZigZagArray(8)))
                {
                    block.ForEach(x => writer.Write(x));
                }
                writer.Write(cbBlocks.Count());
                foreach (var block in cbBlocks.Select(x => x.ToZigZagArray(8)))
                {
                    block.ForEach(x => writer.Write(x));
                }
                writer.Write(crBlocks.Count());
                foreach (var block in crBlocks.Select(x => x.ToZigZagArray(8)))
                {
                    block.ForEach(x => writer.Write(x));
                }
                writer.Write(getSubsamplingCodeByType[subsamplingType]);
                writer.Write(getQuantizingCodeByType[quantizingType]);
                writer.Write(nY);
                writer.Write(nC);
                writer.Write(alphaY);
                writer.Write(gammaY);
                writer.Write(alphaC);
                writer.Write(gammaC);
            }
            Zip7(filename);
        }

        public static void Zip7(string filename)
        {
            var encoder = new SevenZip.SDK.Compress.LZMA.Encoder();
            var outputFilename = filename + "zipped.7z";
            using (var input = new FileStream(filename, FileMode.Open))
            {
                using (var output = new FileStream(outputFilename, FileMode.Create))
                {
                    encoder.Code(input, output, input.Length, -1, null);
                    output.Flush();
                }
            }
        }

        private static Dictionary<QuantizingType, int> getQuantizingCodeByType = new Dictionary<QuantizingType, int>
        {
            {QuantizingType.Nullify, 0},
            {QuantizingType.AlphaGamma, 1},
            {QuantizingType.Recommended, 2}
        };

        private static Dictionary<int, QuantizingType> getQuantizingTypeByCode = new Dictionary<int, QuantizingType>
        {
            {0, QuantizingType.Nullify},
            {1, QuantizingType.AlphaGamma},
            {2, QuantizingType.Recommended}
        }; 

        private static Dictionary<string, int> getSubsamplingCodeByType = new Dictionary<string, int>
        {
            {"2h2v", 0},
            {"2h1v", 1},
            {"1h2v", 2},
            {"1h1v", 3}
        };

        private static Dictionary<int, string> getSubsamplingTypeByCode = new Dictionary<int, string>
        {
            {0, "2h2v"},
            {1, "2h1v"},
            {2, "1h2v"},
            {3, "1h1v"}
        }; 

        private static Dictionary<string, int> getVertSizeLossByType = new Dictionary<string, int>
        {
            {"2h2v", 2},
            {"2h1v", 1},
            {"1h2v", 2},
            {"1h1v", 1}
        };

        private static Dictionary<string, int> getHorSizeLossByType = new Dictionary<string, int>
        {
            {"2h2v", 2},
            {"2h1v", 2},
            {"1h2v", 1},
            {"1h1v", 1}
        }; 
    }
}