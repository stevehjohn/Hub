using System;
using System.IO;
using System.Windows;

namespace Ming.Forms
{
    /// <summary>
    /// Interaction logic for Upgraded.xaml
    /// </summary>
    public partial class Upgraded : Window
    {
        public Upgraded()
        {
            InitializeComponent();

            LoadHistory();
        }

        private void LoadHistory()
        {
            var file = new Uri("pack://application:,,,/Versions.txt", UriKind.Absolute);
            var info = Application.GetResourceStream(file);
            var reader = new StreamReader(info.Stream);
            var text = reader.ReadToEnd();
            reader.Close();
            VersionText.Text = text;
        }
    }
}
