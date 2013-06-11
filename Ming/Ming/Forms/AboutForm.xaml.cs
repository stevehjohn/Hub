using Ming.Infrastructure;
using MingPluginInterfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace Ming.Forms
{
    /// <summary>
    /// Interaction logic for AboutForm.xaml
    /// </summary>
    public partial class AboutForm : Window
    {
        public AboutForm()
        {
            InitializeComponent();
            ShowVersionInfo();
        }

        private void HyperlinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            string site = Properties.Resources.About_WebsiteLink;
            Process.Start(new ProcessStartInfo(site));
            e.Handled = true;
        }

        private ObservableCollection<Tuple<string, string>> _versionInfo;

        private void ShowVersionInfo()
        {
            _versionInfo = new ObservableCollection<Tuple<string, string>>();

            _versionInfo.Add(new Tuple<string, string>(Properties.Resources.App_Title, FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion));

            var interfaces = Assembly.GetAssembly(typeof(ConnectionInfo));
            _versionInfo.Add(new Tuple<string, string>(Properties.Resources.Help_InterfacesName, FileVersionInfo.GetVersionInfo(interfaces.Location).ProductVersion));

            var controls = Assembly.GetAssembly(typeof(MingControls.Extensions.DoubleExtensions));
            _versionInfo.Add(new Tuple<string, string>(Properties.Resources.Help_ControlsName, FileVersionInfo.GetVersionInfo(controls.Location).ProductVersion));

            PluginManager.Instance.Plugins.ToList().ForEach(
                plugin => _versionInfo.Add(new Tuple<string, string>(plugin.Instance.Description, plugin.Instance.Version))
                );

            VersionList.ItemsSource = _versionInfo;
        }
    }
}
