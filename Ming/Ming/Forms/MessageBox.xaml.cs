using System.Windows;

namespace Ming.Forms
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
            Message.Text = message;
            this.ShowDialog();
        }

        public bool ShowConfirm(Window owner, string message)
        {
            this.Owner = owner;
            Title = Properties.Resources.MessageBox_Confirm_DefaultTitle;
            Message.Text = message;
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
