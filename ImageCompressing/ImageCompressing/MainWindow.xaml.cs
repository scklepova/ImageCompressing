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

        private void ToBlackAndWhiteSimple1_OnClick(object sender, RoutedEventArgs e)
        {
            Img1.Source = ToBlackAndWhite((BitmapSource)Img1.Source);
        }

        private void ToBlackAndWhiteSimple2_OnClick(object sender, RoutedEventArgs e)
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

        private BitmapSource ToBlackAndWhiteCCIR(BitmapSource image)
        {
            var pixels = image.ToPixels();
            var stride = image.PixelWidth * 4;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var mid = (pixels[i]*77 + pixels[i + 1]*150 + pixels[i + 2]*29)/256;
                pixels[i] = (byte)(mid);
                pixels[i+1] = (byte)(mid);
                pixels[i+2] = (byte)(mid);
            }

            var bmpSource = BitmapSource.Create(Size, Size, image.DpiX, image.DpiY, image.Format, null, pixels, stride);
            return bmpSource;
        }

        private void ToBlackAndWhite1_OnClick(object sender, RoutedEventArgs e)
        {
            Img1.Source = ToBlackAndWhiteCCIR((BitmapSource) Img1.Source);
        }

        private void ToBlackAndWhite2_OnClick(object sender, RoutedEventArgs e)
        {
            Img2.Source = ToBlackAndWhiteCCIR((BitmapSource)Img2.Source);
        }

        private const int Size = 512;

        private void PsnrButton_OnClick(object sender, RoutedEventArgs e)
        {
            var x = Img1.Source.ToPixels();
            var y = Img2.Source.ToPixels();

            long sum = 0;
            for (var i = 0; i < x.Length; i += 4)
                sum += (x[i] - y[i])*(x[i] - y[i]);
            var psnr = 10*Math.Log10(255.0 * 255 * Size * Size / sum);
            Psnr.Content = psnr;
        }

        private void PsnrButtonColor_OnClick(object sender, RoutedEventArgs e)
        {
            var x = Img1.Source.ToPixels();
            var y = Img2.Source.ToPixels();

            long sum = 0;
            for (var i = 0; i < x.Length; i += 4)
                sum += (x[i] - y[i]) * (x[i] - y[i]);
            var psnr = 10 * Math.Log10(255.0 * 255 * Size * Size * 3 / sum);
            Psnr.Content = psnr;
        }
    }
}
