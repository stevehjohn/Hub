using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Ming.Forms
{
    public partial class LicenceEntry : Window
    {
        public LicenceEntry()
        {
            InitializeComponent();
        }

        private void PurchaseLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            string site = Properties.Resources.LicenceEntry_PurchaseLink;
            Process.Start(new ProcessStartInfo(site));
            e.Handled = true;
        }
    }
}
