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

namespace MingMongoPlugin.Dialogs
{
    public delegate void TargetDatabaseLostFocusEventHandler(string value);

    public partial class CopyCollectionsDialog : Window
    {
        public event TargetDatabaseLostFocusEventHandler TargetDatabaseLostFocus;

        public CopyCollectionsDialog()
        {
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - SystemParameters.WindowCaptionHeight;
            SizeChanged += CopyCollectionsDialogSizeChanged;
        }

        private void CopyCollectionsDialogSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Top + Height > SystemParameters.MaximizedPrimaryScreenHeight - SystemParameters.WindowCaptionHeight)
            {
                Top = SystemParameters.MaximizedPrimaryScreenHeight - SystemParameters.WindowCaptionHeight - Height;
            }
        }

        private void TargetDatabaseComboLostFocus(object sender, RoutedEventArgs e)
        {
            if (TargetDatabaseLostFocus != null)
            {
                var obj = sender as ComboBox;
                if (! string.IsNullOrWhiteSpace(obj.Text))
                {
                    TargetDatabaseLostFocus(obj.Text.Trim());
                }
            }
        }
    }
}
