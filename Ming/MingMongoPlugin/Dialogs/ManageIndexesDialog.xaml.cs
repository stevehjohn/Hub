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
    public partial class ManageIndexesDialog : Window
    {
        private Point _start;

        public ManageIndexesDialog()
        {
            InitializeComponent();

            SizeChanged += ManageIndexesDialogSizeChanged;
        }

        private void ManageIndexesDialogSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void PropertyMouseDown(object sender, MouseButtonEventArgs e)
        {
            _start = e.GetPosition(null);
        }

        private void PropertyMouseMove(object sender, MouseEventArgs e)
        {
            var txt = sender as TextBlock;

            if (txt != null)
            {
                var pos = e.GetPosition(null);
                Vector delta = pos - _start;

                if (e.LeftButton == MouseButtonState.Pressed
                    && (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance
                        || Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    DragDrop.DoDragDrop(txt, txt.Text, DragDropEffects.Copy);
                }
            }
        }
    }
}
