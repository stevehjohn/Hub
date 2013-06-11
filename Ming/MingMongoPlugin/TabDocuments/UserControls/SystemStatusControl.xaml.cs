using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MingMongoPlugin.TabDocuments.UserControls
{
    public partial class SystemStatusControl : UserControl
    {
        public SystemStatusControl()
        {
            InitializeComponent();
        }

        private void LockOptionsGridMouseEnter(object sender, MouseEventArgs e)
        {
            LockOptionsPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void LockOptionsGridMouseLeave(object sender, MouseEventArgs e)
        {
            LockOptionsPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
