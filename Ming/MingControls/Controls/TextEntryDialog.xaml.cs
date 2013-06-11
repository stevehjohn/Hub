using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MingControls.Controls
{
    public partial class TextEntryDialog : Window
    {
        private bool _result = false;

        public TextEntryDialog()
        {
            InitializeComponent();
        }

        private void PositiveClick(object sender, RoutedEventArgs e)
        {
            _result = true;
            Hide();
        }

        public bool Result { get { return _result; } }
    }
}
