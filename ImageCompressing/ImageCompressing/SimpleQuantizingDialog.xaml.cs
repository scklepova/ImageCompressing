using System.Windows;

namespace ImageCompressing
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class SimpleQuantizingDialog : Window
    {
        public SimpleQuantizingDialog()
        {
            InitializeComponent();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public int RedBits { get { return int.Parse(redBits.Text); } }
        public int GreenBits { get { return int.Parse(greenBits.Text); } }
        public int BlueBits { get { return int.Parse(blueBits.Text); } }
    }
}
