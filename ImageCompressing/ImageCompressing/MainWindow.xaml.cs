using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageCompressing.Helpers;
using Microsoft.Win32;
using Color = System.Drawing.Color;
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
            const string peppers = @"C:\Users\Burning Soul\Documents\Сжатие изображений\Test_Images\peppers.png";
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
            var pixels = image.ToPixels();
            var img = new byte[pixels.Length];
            for (var i = 0; i < pixels.Length; i += 4)
            {
                img[i] = (byte)((77*pixels[i] + 150*pixels[i + 1] + 29*pixels[i + 2])/256);
                img[i + 1] = (byte)((-43*pixels[i] -85*pixels[i+1] + 128*pixels[i+2])/256 + 128);
                img[i + 2] = (byte) ((128*pixels[i] - 107*pixels[i+1] - 21*pixels[i+2])/256 + 128);
                
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
                source[i] = (byte)(img[i] + 256*(img[i + 2] - 128)/183);
                long temp = 5329 * ((long)img[i + 1] - 128) + 11103 * ((long)img[i + 2] - 128);
                source[i + 1] = img[i] - temp / 15481 < 0 ? (byte)0 : (byte)(img[i] - temp / 15481);
                source[i + 2] = (byte)(img[i] + 256*(img[i + 1] - 128)/144);
                source[i + 3] = 255;
            }
            return BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, source, image.PixelWidth * 4);
        }

        private BitmapSource ToBlackAndWhiteCCIR(BitmapSource image)
        {
            var pixels = image.ToPixels();
            var stride = image.PixelWidth * 4;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var mid = (pixels[i] * 77 + pixels[i + 1] * 150 + pixels[i + 2] * 29) / 256;
                pixels[i] = (byte)(mid);
                pixels[i + 1] = (byte)(mid);
                pixels[i + 2] = (byte)(mid);
            }

            var bmpSource = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, pixels, stride);
            return bmpSource;
        }

        private byte[] img1;
        private byte[] img2;

        private const int Size = 512;
    }
}
