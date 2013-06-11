using MingControls.General;
using MingMongoPlugin.DataObjects;
using MingMongoPlugin.TabDocuments.UserControls;
using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;

namespace MingMongoPlugin.TabDocuments
{
    internal class SystemStatus : ITabDocument
    {
        private readonly SystemStatusControl _control;
        private readonly ConnectionInfo _cnn;

        private int _instanceId;

        private Timer _timer;
        private double _intervalMultiplier;

        private ServerStatus _curStatus;
        private ServerStatus _prvStatus;

        private string _lockDB;
        private LockType _lockType;

        public SystemStatus(ConnectionInfo cnnInfo)
        {
            _cnn = cnnInfo;
            _control = new SystemStatusControl();

            InitLockOptions();
            SetupCharts();

            _intervalMultiplier = 1;

            _timer = new Timer();
            _timer.Interval = 1000;

            LoadSettings();

            _timer.Elapsed += TimerElapsed;
            _timer.Start();

            _control.Interval.ValueChanged += IntervalValueChanged;
            _control.Span.ValueChanged += SpanValueChanged;
        }

        private void InitLockOptions()
        {
            _control.LockDatabases.Items.Add(new ComboBoxItem { Content = Properties.Resources.ServerStatus_Global, Tag = "", IsSelected = true });
            _control.LockDatabases.SelectionChanged += LockDatabasesSelectionChanged;

            var dbs = MongoUtilities.Create(_cnn).GetDatabaseNames();
            dbs.ToList().ForEach(name => _control.LockDatabases.Items.Add(new ComboBoxItem { Content = name, Tag = name }));

            _lockDB = string.Empty;
            _lockType = LockType.Write;

            _control.ReadLock.Checked += ReadLockChecked;
            _control.WriteLock.Checked += WriteLockChecked;
        }

        void WriteLockChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            _lockType = LockType.Write;
            _control.LocksChart.ChartTitle = ServerStatus.MonitoredLockDescription(_lockDB, _lockType);
            _control.LocksChart.ResetData();
        }

        void ReadLockChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            _lockType = LockType.Read;
            _control.LocksChart.ChartTitle = ServerStatus.MonitoredLockDescription(_lockDB, _lockType);
            _control.LocksChart.ResetData();
        }

        private void SetupCharts()
        {
            _control.MemoryChart.DataPoints = 60;
            _control.MemoryChart.XAxisMaxDesc = Properties.Resources.SystemOverview_Span0;
            _control.MemoryChart.BorderColor = Color.FromRgb(139, 18, 174);
            _control.MemoryChart.FillColor = Color.FromRgb(236, 222, 240);
            _control.MemoryChart.ScaleColor = Color.FromRgb(236, 222, 240);
            _control.MemoryChart.Formatter = new ByteNumberFormatter();
            _control.MemoryChart.ChartTitle = Properties.Resources.SystemOverview_MemoryTitle;

            _control.NetworkInChart.DataPoints = 60;
            _control.NetworkInChart.XAxisMaxDesc = Properties.Resources.SystemOverview_Span0;
            _control.NetworkInChart.BorderColor = Color.FromRgb(17, 125, 187);
            _control.NetworkInChart.FillColor = Color.FromRgb(206, 226, 240);
            _control.NetworkInChart.ScaleColor = Color.FromRgb(206, 226, 240);
            _control.NetworkInChart.Formatter = new ByteNumberFormatter();
            _control.NetworkInChart.ChartTitle = Properties.Resources.SystemOverview_NetworkIn;

            _control.NetworkOutChart.DataPoints = 60;
            _control.NetworkOutChart.XAxisMaxDesc = Properties.Resources.SystemOverview_Span0;
            _control.NetworkOutChart.BorderColor = Color.FromRgb(77, 166, 12);
            _control.NetworkOutChart.FillColor = Color.FromRgb(206, 231, 188);
            _control.NetworkOutChart.ScaleColor = Color.FromRgb(206, 231, 188);
            _control.NetworkOutChart.Formatter = new ByteNumberFormatter();
            _control.NetworkOutChart.ChartTitle = Properties.Resources.SystemOverview_NetworkOut;

            _control.ConnectionsChart.DataPoints = 60;
            _control.ConnectionsChart.XAxisMaxDesc = Properties.Resources.SystemOverview_Span0;
            _control.ConnectionsChart.BorderColor = Color.FromRgb(167, 79, 1);
            _control.ConnectionsChart.FillColor = Color.FromRgb(238, 222, 207);
            _control.ConnectionsChart.ScaleColor = Color.FromRgb(238, 222, 207);
            _control.ConnectionsChart.Formatter = new NumberFormatter();
            _control.ConnectionsChart.ChartTitle = Properties.Resources.SystemOverview_Connections;
            _control.ConnectionsChart.ScaleIsWholeNumber = true;

            _control.CurrentOpsChart.DataPoints = 60;
            _control.CurrentOpsChart.XAxisMaxDesc = Properties.Resources.SystemOverview_Span0;
            _control.CurrentOpsChart.BorderColor = Color.FromRgb(0, 158, 142);
            _control.CurrentOpsChart.FillColor = Color.FromRgb(93, 207, 195);
            _control.CurrentOpsChart.ScaleColor = Color.FromRgb(93, 207, 195);
            _control.CurrentOpsChart.Formatter = new NumberFormatter();
            _control.CurrentOpsChart.ChartTitle = Properties.Resources.SystemOverview_TotalOps;
            _control.CurrentOpsChart.ScaleIsWholeNumber = true;

            _control.LocksChart.DataPoints = 60;
            _control.LocksChart.XAxisMaxDesc = Properties.Resources.SystemOverview_Span0;
            _control.LocksChart.BorderColor = Color.FromRgb(255, 35, 0);
            _control.LocksChart.FillColor = Color.FromRgb(255, 134, 115);
            _control.LocksChart.ScaleColor = Color.FromRgb(255, 134, 115);
            _control.LocksChart.Formatter = new MicrosecondNumberFormatter();
            _control.LocksChart.ChartTitle = ServerStatus.MonitoredLockDescription(_lockDB, _lockType);
        }

        private void LockDatabasesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _lockDB = (string)(_control.LockDatabases.SelectedItem as Control).Tag;
            _control.LocksChart.ChartTitle = ServerStatus.MonitoredLockDescription(_lockDB, _lockType);
            _control.LocksChart.ResetData();
        }

        private void UpdateChartSpans(int points, string xAxisDesc)
        {
            _control.MemoryChart.DataPoints = points;
            _control.MemoryChart.XAxisMaxDesc = xAxisDesc;
            _control.MemoryChart.ResetYAxisScale();
            _control.MemoryChart.RefreshChart();

            _control.NetworkInChart.DataPoints = points;
            _control.NetworkInChart.XAxisMaxDesc = xAxisDesc;
            _control.NetworkInChart.ResetYAxisScale();
            _control.NetworkInChart.RefreshChart();

            _control.NetworkOutChart.DataPoints = points;
            _control.NetworkOutChart.XAxisMaxDesc = xAxisDesc;
            _control.NetworkOutChart.ResetYAxisScale();
            _control.NetworkOutChart.RefreshChart();

            _control.ConnectionsChart.DataPoints = points;
            _control.ConnectionsChart.XAxisMaxDesc = xAxisDesc;
            _control.ConnectionsChart.ResetYAxisScale();
            _control.ConnectionsChart.RefreshChart();

            _control.CurrentOpsChart.DataPoints = points;
            _control.CurrentOpsChart.XAxisMaxDesc = xAxisDesc;
            _control.CurrentOpsChart.ResetYAxisScale();
            _control.CurrentOpsChart.RefreshChart();

            _control.LocksChart.DataPoints = points;
            _control.LocksChart.XAxisMaxDesc = xAxisDesc;
            _control.LocksChart.ResetYAxisScale();
            _control.LocksChart.RefreshChart();
        }

        private void LoadSettings()
        {
            var val = 0;
            val = Properties.Settings.Default.SystemOverviewInterval;
            _control.Interval.Value = val;
            val = Properties.Settings.Default.SystemOverviewSpan;
            _control.Span.Value = val;

            IntervalValueChanged(null, null);
            SpanValueChanged(null, null);
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.SystemOverviewInterval = (int) _control.Interval.Value;
            Properties.Settings.Default.SystemOverviewSpan = (int)_control.Span.Value;
            Properties.Settings.Default.Save();
        }

        private int GetDataPointCount(int intervalNo)
        {
            switch (intervalNo)
            {
                case 1:
                    return (int)(600 * _intervalMultiplier);
                case 2:
                    return (int)(1800 * _intervalMultiplier);
                case 3:
                    return (int)(3600 * _intervalMultiplier);
                default:
                    return (int)(60 * _intervalMultiplier);
            }
        }

        private void SpanValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            var val = (int) _control.Span.Value;
            _control.SpanDesc.Text = Properties.Resources.ResourceManager.GetString(string.Format("SystemOverview_Span{0}", val));

            UpdateChartSpans(GetDataPointCount(val), Properties.Resources.ResourceManager.GetString(string.Format("SystemOverview_Span{0}", val)));

            SaveSettings();
        }

        private void IntervalValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            var val = (int)_control.Interval.Value;
            _control.IntervalDesc.Text = Properties.Resources.ResourceManager.GetString(string.Format("SystemOverview_Interval{0}", val));

            switch (val)
            {
                case 0:
                    _timer.Interval = 500;
                    break;
                case 2:
                    _timer.Interval = 10000;
                    break;
                case 3:
                    _timer.Interval = 30000;
                    break;
                case 4:
                    _timer.Interval = 60000;
                    break;
                default:
                    _timer.Interval = 1000;
                    break;
            }
            _intervalMultiplier = 1000.0 / _timer.Interval;
            var spanVal = (int)_control.Span.Value;
            UpdateChartSpans(GetDataPointCount(spanVal), Properties.Resources.ResourceManager.GetString(string.Format("SystemOverview_Span{0}", spanVal)));

            SaveSettings();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            LoadSystemStatus();
            _control.Dispatcher.Invoke(new Action(() => 
                {
                    _control.MemoryChart.AddDataPoint(_curStatus.VirtualMemoryMB * 1024);
                    _control.ConnectionsChart.AddDataPoint(_curStatus.Connections);
                }));
            if (_prvStatus != null)
            {
                var diff = _curStatus - _prvStatus;
                _control.Dispatcher.Invoke(new Action(() =>
                {
                    _control.NetworkInChart.AddDataPoint(diff.NetworkBytesIn);
                    _control.NetworkOutChart.AddDataPoint(diff.NetworkBytesOut);
                    _control.CurrentOpsChart.AddDataPoint(diff.TotalOps);
                    _control.LocksChart.AddDataPoint(_curStatus.GetLockTimeMicros(_lockDB, _lockType) - _prvStatus.GetLockTimeMicros(_lockDB, _lockType));
                }));
            }
        }

        private void LoadSystemStatus()
        {
            try
            {
                var status = MongoUtilities.Create(_cnn).GetDatabase("local").RunCommand("serverStatus");
                _prvStatus = _curStatus;
                _curStatus = new ServerStatus(status.Response.AsBsonDocument);
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        public System.Windows.Controls.UserControl Control
        {
            get { return _control; }
        }

        public string Title
        {
            get { return string.Format(Properties.Resources.SystemStatus_Title, _cnn.Host, _cnn.Port); }
        }

        public string Description
        {
            get { return string.Format(Properties.Resources.SystemStatus_Description, _cnn.Host, _cnn.Port); }
        }

        public bool CloseRequested()
        {
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
