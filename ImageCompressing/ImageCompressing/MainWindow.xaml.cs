using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageCompressing.Helpers;
using Microsoft.Win32;

using Image = System.Windows.Controls.Image;

namespace ImageCompressing
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            const string peppers = @"C:\Users\Burning Soul\Documents\Сжатие изображений\Test_Images\baboon.png";
            Img1.Source = new BitmapImage(new Uri(peppers));
            Img2.Source = new BitmapImage(new Uri(peppers));
        }

        private void LoadImage1_OnClick(object sender, RoutedEventArgs e)
        {
            OpenImage(Img1);
        }

        private void LoadImage2_OnClick(object sender, RoutedEventArgs e)
        {
            OpenImage(Img2);
        }

        private void CopyImage1_OnClick(object sender, RoutedEventArgs e)
        {
            Img1.Source = Img2.Source;
        }

        private void CopyImage2_OnClick(object sender, RoutedEventArgs e)
        {
            Img2.Source = Img1.Source;
        }

        private void OpenImage(Image image)
        {
            var opd = new OpenFileDialog
            {
                Title = "Select an image",
                Filter =    "All supported graphics|*.jpg;*.jpeg;*.png|" +
                            "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                            "Portable Network Graphic (*.png)|*.png"
            };

            if (opd.ShowDialog() == true)
            {
                image.Source = new BitmapImage(new Uri(opd.FileName));
            }
        }

        private void ToGrayscaleSimple1_OnClick(object sender, RoutedEventArgs e)
        {
            Img1.Source = ToBlackAndWhite((BitmapSource)Img1.Source);
        }

        private void ToGrayscaleSimple2_OnClick(object sender, RoutedEventArgs e)
        {
            Img2.Source = ToBlackAndWhite((BitmapSource)Img2.Source);
        }

        private BitmapSource ToBlackAndWhite(BitmapSource image)
        {
            var pixels = image.ToPixels();
            var stride = image.PixelWidth * 4;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var mid = (pixels[i + 1] + pixels[i + 2] + pixels[i]) / 3;
                pixels[i] = (byte)mid;
                pixels[i + 1] = (byte)mid;
                pixels[i + 2] = (byte)mid;
            }

            var bmpSource = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, pixels, stride);
            return bmpSource;

//            label.Content = "done";
        }

        private void ToGrayscale1_OnClick(object sender, RoutedEventArgs e)
        {
            Img1.Source = ToBlackAndWhiteCCIR((BitmapSource) Img1.Source);
        }

        private void ToGrayscale2_OnClick(object sender, RoutedEventArgs e)
        {
            Img2.Source = ToBlackAndWhiteCCIR((BitmapSource)Img2.Source);
        }

        private void Img1Y_OnClick(object sender, RoutedEventArgs e)
        {
            img1 = ToYCbCr((BitmapSource)Img1.Source);
            Img1.Source = VisualizeYCbCr(img1, (BitmapSource) Img1.Source, 0);
        }

        private void Img1Cb_OnClick(object sender, RoutedEventArgs e)
        {
            img1 = ToYCbCr((BitmapSource)Img1.Source);
            Img1.Source = VisualizeYCbCr(img1, (BitmapSource)Img1.Source, 1);
        }

        private void Img1Cr_OnClick(object sender, RoutedEventArgs e)
        {
            img1 = ToYCbCr((BitmapSource)Img1.Source);
            Img1.Source = VisualizeYCbCr(img1, (BitmapSource)Img1.Source, 2);
        }

        private void Img1_ToRGB(object sender, RoutedEventArgs e)
        {
            Img1.Source = ToRGB(img1, (BitmapSource) Img1.Source);
        }

        private void MedianCut_OnClick(object sender, RoutedEventArgs e)
        {
            var image = (BitmapSource) Img1.Source;
            var cutted = QuantizingMaster.MedianCut(image.ToPixels(), 10);
            Img1.Source = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, cutted, image.PixelWidth * 4);
        }

        private void LBG_OnClick(object sender, RoutedEventArgs e)
        {
            var image = (BitmapSource)Img1.Source;
            var cutted = QuantizingMaster.LBG(image.ToPixels(), 8, 256);
            Img1.Source = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, cutted, image.PixelWidth * 4);
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            var test = sender as Image;
            var image = (BitmapSource)Img1.Source;
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save an image"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }
            };
        }

        /**********************/

        private void PsnrButton_OnClick(object sender, RoutedEventArgs e)
        {
            var x = Img1.Source.ToPixels();
            var y = Img2.Source.ToPixels();

            long sum = 0;
            for (var i = 0; i < x.Length; i += 4)
                sum += (x[i] - y[i]) * (x[i] - y[i]) 
                    + (x[i + 1] - y[i + 1]) * (x[i + 1] - y[i + 1]) 
                    + (x[i + 2] - y[i + 2]) * (x[i + 2] - y[i + 2]);
            var psnr = 10*Math.Log10(3 * 255.0 * 255 * Size * Size / sum);
            Psnr.Content = psnr;
        }

        private byte[] ToYCbCr(BitmapSource image)
        {
            //BGR а не RGB
            var pixels = image.ToPixels();
            var img = new byte[pixels.Length];
            for (var i = 0; i < pixels.Length; i += 4)
            {                
                img[i] = GetByteValue((77*pixels[i + 2] + 150*pixels[i + 1] + 29*pixels[i])/256);
                img[i + 1] = GetByteValue((-43*pixels[i + 2] -85*pixels[i+1] + 128*pixels[i])/256 + 128);
                img[i + 2] = GetByteValue((128*pixels[i + 2] - 107*pixels[i+1] - 21*pixels[i])/256 + 128);

            }
            return img;
        }

        private BitmapSource VisualizeYCbCr(byte[] img, BitmapSource image, int offset)
        {
            var source = new byte[img.Length];
            for (var i = 0; i < img.Length; i += 4)
            {
                source[i] = img[i + offset];
                source[i + 1] = img[i + offset];
                source[i + 2] = img[i + offset];
                source[i + 3] = 255;
            }

            return BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, source, image.PixelWidth*4);
        }

        private BitmapSource ToRGB(byte[] img, BitmapSource image)
        {
            var source = new byte[img.Length];
            for (var i = 0; i < img.Length; i += 4)
            {
                long temp = img[i] + 256*(img[i + 2] - 128)/183;
                source[i + 2] = GetByteValue(temp);

                temp = 5329 * ((long)img[i + 1] - 128) + 11103 * ((long)img[i + 2] - 128);
                source[i + 1] = GetByteValue(img[i] - temp / 15481);

                temp = img[i] + 256*(img[i + 1] - 128)/144;
                source[i] = GetByteValue(temp);
                source[i + 3] = 255;
            }
            return BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, source, image.PixelWidth * 4);
        }

        private byte GetByteValue(long num)
        {
            if (num < 0)
                return 0;
            if (num > 255)
                return 255;
            return (byte) num;
        }

        private BitmapSource ToBlackAndWhiteCCIR(BitmapSource image)
        {
            var pixels = image.ToPixels();
            var stride = image.PixelWidth * 4;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var mid = (pixels[i + 2] * 77 + pixels[i + 1] * 150 + pixels[i] * 29) / 256;
                pixels[i] = (byte)(mid);
                pixels[i + 1] = (byte)(mid);
                pixels[i + 2] = (byte)(mid);
            }

            var bmpSource = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, pixels, stride);
            return bmpSource;
        }

        private void UniformQuantizing_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleQuantizingDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                var image = (BitmapSource) Img1.Source;
                var pixels = Img1.Source.ToPixels();
                pixels = QuantizingMaster.UniformQuantization(pixels, dialog.RedBits, dialog.GreenBits, dialog.BlueBits);
                Img1.Source = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, pixels,
                    image.PixelWidth*4);
            }
        }

        private void UniformQuantizing_YCbCr_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleQuantizingDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                var image = (BitmapSource) Img1.Source;
                var pixels = ToYCbCr(image);
                pixels = QuantizingMaster.UniformQuantization(pixels, dialog.BlueBits, dialog.GreenBits, dialog.RedBits);
                Img1.Source = ToRGB(pixels, image);
            }
        }

        private byte[] img1;
        private byte[] img2;

        private const int Size = 512;


        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var image = (BitmapSource) Img1.Source;
            var pixels = image.ToPixels();
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 0;
                pixels[i + 1] = 255;
                pixels[i + 2] = 0;
            }
            Img1.Source = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, pixels,
                    image.PixelWidth * 4);
        }

        private void MenuItem_Downsampling(object sender, RoutedEventArgs e)
        {
            var type = "2h1v";
            var image = (BitmapSource)Img1.Source;
            var pixels = ToYCbCr(image);
            var matrix = ToMatrix(pixels);
            
            for(var i = 0; i < Size; i += 2)
                for (var j = 0; j < Size*4; j += 8)
                {
                    matrix[i][j + 4 + 1] = matrix[i][j + 1];
                    matrix[i][j + 4 + 2] = matrix[i][j + 2];

                    matrix[i + 1][j + 4 + 1] = matrix[i + 1][j + 1];
                    matrix[i + 1][j + 4 + 2] = matrix[i + 1][j + 2];
                }
            pixels = MatrixToPixels(matrix);
            Img1.Source = VisualizeYCbCr(pixels, (BitmapSource)Img1.Source, 2);
        }

        private byte[][] ToMatrix(byte[] pixels)
        {
            var i = -1;
            var matrix = new byte[Size][];
            for (var k = 0; k < pixels.Length; k += 4)
            {
                var j = k%(Size*4);
                if (j == 0)
                {
                    i++;
                    matrix[i] = new byte[Size*4];
                }

                matrix[i][j] = pixels[k];
                matrix[i][j + 1] = pixels[k + 1];
                matrix[i][j + 2] = pixels[k + 2];
                matrix[i][j + 3] = pixels[k + 3];
            }

            return matrix;
        }

        private byte[] MatrixToPixels(byte[][] matrix)
        {
            var pixels = new byte[Size * Size * 4];
            for(var i = 0; i < Size; i++)
                for (var j = 0; j < Size*4; j++)
                    pixels[i*Size*4 + j] = matrix[i][j];
            return pixels;
        }
    }
}
