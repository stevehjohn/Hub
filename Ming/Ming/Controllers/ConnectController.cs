using System;
using System.Windows;
using Ming.Forms;
using Ming.Infrastructure;
using Ming.Infrastructure.Interfaces;
using Ming.ViewModels;
using MingPluginInterfaces;
using System.Windows.Controls;

namespace Ming.Controllers
{
    internal class ConnectController
    {
        private readonly Window _owner;
        private readonly ConnectForm _connectForm;
        private readonly ConnectViewModel _viewModel;

        private ProgressDialog _progressDialog = null;

        public ConnectController(Window owner)
        {
            _owner = owner;
            _connectForm = new ConnectForm();
            _viewModel = (ConnectViewModel)_connectForm.DataContext;
            _viewModel.DialogComplete += new DialogCompleteEventHandler(viewModel_DialogComplete);
            _viewModel.TestStarted += new EventHandler(viewModel_TestStarted);
            _viewModel.TestComplete += new EventHandler<EventArgs<bool>>(viewModel_TestComplete);
            _viewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(viewModel_PropertyChanged);
            _connectForm.Owner = _owner;
            _connectForm.Password.PasswordChanged += PasswordChanged;
        }

        void PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = ((PasswordBox)sender).SecurePassword;
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _connectForm.TestStatusImage.Source = Utilities.BitmapImageFromBitmap(Properties.Resources.warning);
        }

        void viewModel_TestComplete(object sender, EventArgs<bool> e)
        {
            if (_progressDialog != null)
            {
                _progressDialog.Dispatcher.Invoke(new Action(HideProgressDialog), null);
            }
            if (e.Result)
            {
                _connectForm.TestStatusImage.Source = Utilities.BitmapImageFromBitmap(Properties.Resources.ok);
            }
            else
            {
                _connectForm.TestStatusImage.Source = Utilities.BitmapImageFromBitmap(Properties.Resources.error);
            }
        }

        private void viewModel_TestStarted(object sender, EventArgs e)
        {
            _progressDialog = new ProgressDialog();
            _progressDialog.Owner = _connectForm;
            _progressDialog.Title = Properties.Resources.Connect_Progress_Testing;
            if (!_progressDialog.ShowDialog() ?? false)
            {
                _viewModel.CancelTest();
            }
            _progressDialog = null;
        }

        public void viewModel_DialogComplete(object sender)
        {
            _connectForm.Close();
        }

        public void HideProgressDialog()
        {
            if (_progressDialog != null)
            {
                if (_progressDialog.IsVisible)
                    _progressDialog.Hide();
                _progressDialog = null;
            }
        }

        public void Show()
        {
            _connectForm.ShowDialog();

            if (_viewModel.ShouldAdd)
            {
                var cnnInfo = new ConnectionInfo(_viewModel.Name, _viewModel.Service.Id, _viewModel.Host, int.Parse(_viewModel.Port), _viewModel.Username, _viewModel.Password);

                try
                {
                    SettingsManager.Instance.AddConnection(cnnInfo);
                }
                catch (ArgumentException)
                {
                    new Forms.MessageBox().ShowMessage(_owner, 
                        string.Format(Properties.Resources.Connect_Error_Duplicate, 
                            string.Format("{0}:{1}", cnnInfo.Host, cnnInfo.Port),
                            _viewModel.Service.Name), 
                        Properties.Resources.Connect_Error_Title);
                }
            }
        }
    }
}
