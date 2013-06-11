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

namespace MingPluginInterfaces.Forms
{
    public partial class MessageBox : Window
    {
        private bool _result = false;

        public MessageBox()
        {
            InitializeComponent();
        }

        public void ShowMessage(Window owner, string message, string title)
        {
            this.Owner = owner;
            Title = title;
            Message.Inlines.AddRange(ParseMessageForNewLines(message));
            this.ShowDialog();
        }

        private IEnumerable<Inline> ParseMessageForNewLines(string message)
        {
            return message.Split(new [] { "\\n" }, StringSplitOptions.None).ToList().Select(part => string.IsNullOrWhiteSpace(part) ? (Inline)new LineBreak() : (Inline)new Run(part));
        }

        public bool ShowConfirm(Window owner, string message)
        {
            this.Owner = owner;
            Title = Properties.Resources.MessageBox_Confirm_DefaultTitle;
            Message.Inlines.AddRange(ParseMessageForNewLines(message));
            PositiveText.Text = Properties.Resources.MessageBox_YesButton;
            Positive.IsCancel = false;
            Negative.IsCancel = true;
            Negative.Visibility = System.Windows.Visibility.Visible;
            this.ShowDialog();
            return _result;
        }

        private void Positive_Click(object sender, RoutedEventArgs e)
        {
            _result = true;
            this.Hide();
        }
    }
}
