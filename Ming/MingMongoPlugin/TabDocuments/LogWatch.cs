using MingControls.Controls;
using MingControls.Extensions;
using MingMongoPlugin.TabDocuments.UserControls;
using MingPluginInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using System.Linq;

namespace MingMongoPlugin.TabDocuments
{
    internal class LogWatch : ITabDocument
    {
        private int _viewSize = 500;

        private readonly LogWatchControl _control;
        private readonly ConnectionInfo _cnn;
        private readonly ObservableCollection<string> _logs;
        private readonly List<LogEntry> _entries;

        private int _instanceId;

        private Timer _timer;
        private string _logName;

        public LogWatch(ConnectionInfo cnn)
        {
            _cnn = cnn;
            _control = new LogWatchControl();
            _control.LogViewer.ViewSize = _viewSize;
            _logs = new ObservableCollection<string>();
            _control.LogName.ItemsSource = _logs;
            _control.LogName.SelectionChanged += LogNameSelectionChanged;

            _control.PauseButton.Click += PauseButtonClick;

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += TimerElapsed;

            _entries = new List<LogEntry>();

            LoadAvailableLogs();
            GetLogLevel();
        }

        private void PauseButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var paused = (bool) _control.PauseButton.IsChecked;

            if (paused)
            {
                _timer.Stop();
                _control.PauseButton.ToolTip = Properties.Resources.LogWatch_Resume;
            }
            else
            {
                _timer.Start();
                _control.PauseButton.ToolTip = Properties.Resources.LogWatch_Pause;
            }
        }

        private void GetLogLevel()
        {
            var task = new CancelableTask(DoGetLogLevel, null);
            task.Execute();
        }

        private void DoGetLogLevel()
        {
            try
            {
                var cmd = new CommandDocument("getParameter", new BsonInt32(1));
                cmd.Add("logLevel", new BsonInt32(1));
                var response = MongoUtilities.Create(_cnn).GetDatabase("admin").RunCommand(cmd).Response;
                var level = response["logLevel"].AsInt32;
                _control.Dispatcher.Invoke(new Action(() =>
                    {
                        _control.LogLevel.Value = level;
                        _control.LogLevel.ValueChanged += LogLevelValueChanged;
                    }));
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        private void LogLevelValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            SetLogLevel((int) _control.LogLevel.Value);
        }

        private void SetLogLevel(int level)
        {
            var task = new CancelableTask(() => DoSetLogLevel(level), null);
            task.Execute();
        }

        private void DoSetLogLevel(int level)
        {
            try
            {
                var cmd = new CommandDocument("setParameter", new BsonInt32(1));
                cmd.Add("logLevel", new BsonInt32(level));
                var response = MongoUtilities.Create(_cnn).GetDatabase("admin").RunCommand(cmd).Response;
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        private void LogNameSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _logName = (string) _control.LogName.SelectedValue;
            _entries.Clear();
            _control.LogViewer.ClearEntries();
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            LoadLogEntries();
        }

        private void LoadAvailableLogs()
        {
            var task = new CancelableTask(DoLoadAvailableLogs, null);
            task.Execute();
        }

        private void DoLoadAvailableLogs()
        {
            try
            {
                var logs = MongoUtilities.Create(_cnn).GetDatabase("admin").RunCommand(new CommandDocument("getLog", new BsonString("*"))).Response;
                _control.Dispatcher.Invoke(new Action(() =>
                    {
                        logs["names"].AsBsonArray.ToList().ForEach(log => _logs.Add(log.AsString));
                        _control.LogName.SelectedIndex = 0;
                        _logName = logs["names"].AsBsonArray[0].AsString;
                    }));
                _timer.Start();
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        private void LoadLogEntries()
        {
            var log = MongoUtilities.Create(_cnn).GetDatabase("admin").RunCommand(new CommandDocument("getLog", new BsonString(_logName))).Response;
            var source = log["log"].AsBsonArray.ToList();

            source.Reverse();
            source = source.Take(_viewSize).ToList();
            source.Reverse();

            _entries.ForEach(item => item.IsNew = false);

            source.ForEach(item => 
                {
                    var entry = ParseLogItem(item.AsString);
                    if (entry != null)
                    {
                        entry.IsNew = true;
                        if (!_entries.Contains(entry))
                        {
                            _entries.Insert(0, entry);
                        }
                    }
                });

            while (_entries.Count > _viewSize)
            {
                _entries.RemoveAt(_entries.Count - 1);
            }

            _control.Dispatcher.Invoke(new Action(() => _control.LogViewer.ReplaceEntries(_entries)));
        }

        private LogEntry ParseLogItem(string item)
        {
            try
            {
                var idx = item.NthIndexOf(' ', 4);
                var timeStr = item.Substring(0, idx).Trim();
                timeStr = timeStr.Substring(timeStr.IndexOf(' ') + 1);
                timeStr = string.Format("{0} {1}", DateTime.UtcNow.Year.ToString(), timeStr);
                var time = DateTime.Parse(timeStr);
                var desc = item.Substring(idx + 1);
                return new LogEntry(time, desc);
            }
            catch
            {
            }
            return null;
        }

        public System.Windows.Controls.UserControl Control
        {
            get { return _control; }
        }

        public string Title
        {
            get { return string.Format(Properties.Resources.LogWatch_Title, _cnn.Host, _cnn.Port); }
        }

        public string Description
        {
            get { return string.Format(Properties.Resources.LogWatch_Description, _cnn.Host, _cnn.Port); }
        }

        public bool CloseRequested()
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
            return true;
        }

        public int InstanceId
        {
            set { _instanceId = value; }
        }

        public void MenuItemClicked(string menuKey)
        {
            throw new NotImplementedException();
        }
    }
}
