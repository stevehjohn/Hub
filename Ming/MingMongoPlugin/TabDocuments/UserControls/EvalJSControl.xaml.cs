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
    public delegate void EvaluateClickedEventHandler();

    public partial class EvalJSControl : UserControl
    {
        public event EvaluateClickedEventHandler EvaluateClicked;

        public EvalJSControl()
        {
            InitializeComponent();
            SelectColor(ForegroundCombo, Output.Foreground);
            SelectColor(BackgroundCombo, Output.Background);
        }

        public Color ForegroundColor
        {
            get
            {
                return (Output.Foreground as SolidColorBrush).Color;
            }
            set
            {
                SelectColor(ForegroundCombo, new SolidColorBrush(value));
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return (Output.Background as SolidColorBrush).Color;
            }
            set
            {
                SelectColor(BackgroundCombo, new SolidColorBrush(value));
            }
        }

        private void EvaluateClick(object sender, RoutedEventArgs e)
        {
            EvaluateClicked();
        }

        private void SelectColor(ComboBox combo, Brush value)
        {
            var col = (value as SolidColorBrush).Color;
            foreach (ComboBoxItem item in combo.Items)
            {
                var rect = item.Content as Rectangle;
                if ((rect.Fill as SolidColorBrush).Color == col)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void BackgroundSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var cbi = e.AddedItems[0] as ComboBoxItem;
                if (cbi != null)
                {
                    var rect = cbi.Content as Rectangle;
                    if (rect != null)
                    {
                        Output.Background = rect.Fill;
                        JavaScript.Background = rect.Fill;
                    }
                }
            }
        }

        private void ForegroundSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var cbi = e.AddedItems[0] as ComboBoxItem;
                if (cbi != null)
                {
                    var rect = cbi.Content as Rectangle;
                    if (rect != null)
                    {
                        Output.Foreground = rect.Fill;
                        JavaScript.Foreground = rect.Fill;
                    }
                }
            }
        }
    }
}
