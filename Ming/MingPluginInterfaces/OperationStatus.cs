using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Windows;

namespace MingPluginInterfaces
{
    public class OperationStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _description;
        private bool _isComplete;
        private bool _isIndeterminate;
        private bool _isSuccess;
        private int _percentComplete;
        private string _statusText = Properties.Resources.StatusText_Failed;
        private string _title;

        public string Description 
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Description"));
            }
        }

        public bool IsComplete
        {
            get
            {
                return _isComplete;
            }
            set
            {
                _isComplete = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsComplete"));
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StatusTextVisibility"));
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ProgressBarVisibility"));
            }
        }

        public bool IsIndeterminate
        {
            get
            {
                return _isIndeterminate;
            }
            set
            {
                _isIndeterminate = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsIndeterminate"));
            }
        }

        public bool IsSuccess
        {
            get
            {
                return _isSuccess;
            }
            set
            {
                _isSuccess = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSuccess"));
                StatusText = _isSuccess ? Properties.Resources.StatusText_Complete : Properties.Resources.StatusText_Failed;
            }
        }

        public int PercentComplete
        {
            get
            {
                return _percentComplete;
            }
            set
            {
                _percentComplete = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PercentComplete"));
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return _isComplete ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
            }
        }

        public Visibility StatusTextVisibility
        {
            get
            {
                return _isComplete ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Title"));
            }
        }
    }
}
