using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Ming.Infrastructure;
using Ming.Infrastructure.Interfaces;
using MingPluginInterfaces;
using System.Security;

namespace Ming.ViewModels
{
    internal class ConnectViewModel : INotifyPropertyChanged, IDataErrorInfo, INotifyDialogComplete
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event DialogCompleteEventHandler DialogComplete;
        public event EventHandler TestStarted;
        public event EventHandler<EventArgs<bool>> TestComplete;

        private string _name;
        private PluginInfo _service;
        private string _host;
        private string _port;
        private string _username;
        private SecureString _password;
        private bool _shouldAdd;
        private CancelableTask<bool> _testTask;

        private ICommand _addConnectionCommand;
        private ICommand _testConnectionCommand;

        public ConnectViewModel()
        {
            _service = PluginManager.Instance.Plugins[0];
            _port = _service.Instance.DefaultPort.ToString();
        }

        public ICommand AddConnectionCommand
        {
            get 
            {
                if (_addConnectionCommand == null)
                    _addConnectionCommand = new RelayCommand(p => this.AddConnection(), p => this.CanProceed);
                return _addConnectionCommand; 
            }
        }

        public ICommand TestConnectionCommand
        {
            get
            {
                if (_testConnectionCommand == null)
                    _testConnectionCommand = new RelayCommand(p => this.TestConnection(), p => this.CanProceed);
                return _testConnectionCommand;
            }
        }

        public void AddConnection()
        {
            _shouldAdd = true;
            DialogComplete(this);
        }

        public bool ShouldAdd
        {
            get { return _shouldAdd; }
        }

        public void TestConnection()
        {
            var plugin = PluginManager.Instance.GetPluginInstance(_service.Id);

            var cnn = new ConnectionInfo(_name, _service.Id, _host, int.Parse(_port), _username, _password);

            _testTask = new CancelableTask<bool>(() => plugin.Test(cnn), TestFinished).Execute();

            TestStarted(this, null);
        }

        private void TestFinished(bool result)
        {
            System.Diagnostics.Debug.WriteLine(result.ToString());
            TestComplete(this, new EventArgs<bool>(result));
        }

        public void CancelTest()
        {
            _testTask.Cancel();
        }

        public bool CanProceed
        {
            get
            {
                return this["Host"] == null && this["Port"] == null;
            }
        }

        public IList<PluginInfo> PlugIns
        {
            get { return PluginManager.Instance.Plugins; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public PluginInfo Service
        {
            get { return _service; }
            set 
            { 
                _service = value;
                Port = _service.Instance.DefaultPort.ToString();
                PropertyChanged(this, new PropertyChangedEventArgs("Service"));
            }
        }

        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Host"));
            }
        }

        public string Port
        {
            get { return _port; }
            set 
            { 
                _port = value; 
                PropertyChanged(this, new PropertyChangedEventArgs("Port"));
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Username"));
            }
        }

        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Password"));
            }
        }

        public string Error
        {
            get { throw new System.NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                switch (columnName)
                {
                    case "Host":
                        if (string.IsNullOrWhiteSpace(Host))
                            result = Properties.Resources.Validator_Host_Error;
                        break;
                    case "Port":
                        if (!Validation.SimpleValidation.IsIntInRange(_port, 0, 65535))
                            result = Properties.Resources.Validator_Port_Error;
                        break;
                }
                return result;
            }
        }
    }
}
