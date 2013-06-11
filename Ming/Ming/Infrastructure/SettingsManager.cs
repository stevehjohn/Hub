using System;
using System.Linq;
using System.Collections.Generic;
using MingPluginInterfaces;
using System.Windows;

namespace Ming.Infrastructure
{
    public enum SettingsChangedEventType
    {
        Added,
        Updated,
        Deleted
    }

    public class SettingsChangedEventArgs<TSetting>
    {
        private readonly SettingsChangedEventType _eventType;
        private readonly TSetting _setting;

        public SettingsChangedEventArgs(SettingsChangedEventType eventType, TSetting setting)
        {
            _eventType = eventType;
            _setting = setting;
        }

        public SettingsChangedEventType EventType { get { return _eventType; } }
        public TSetting Setting { get { return _setting; } }
    }

    public delegate void SettingChangedEventHandler<TSetting>(object sender, SettingsChangedEventArgs<TSetting> args);

    internal class SettingsManager
    {
        private List<ConnectionInfo> _connections;
        public event SettingChangedEventHandler<ConnectionInfo> ConnectionChanged;

        public WindowState WindowState
        {
            get
            {
                WindowState state;
                if (!Enum.TryParse(Properties.Settings.Default.WindowState, out state))
                {
                    state = WindowState.Normal;
                }
                return state;
            }
            set
            {
                Properties.Settings.Default.WindowState = value.ToString();
                Properties.Settings.Default.Save();
            }
        }

        public Rect WindowPosition
        {
            get
            {
                Rect r = new Rect(0, 0, 0, 0);
                try
                {
                    r = Rect.Parse(Properties.Settings.Default.WindowPosition);
                }
                catch { }
                return r;
            }
            set
            {
                Properties.Settings.Default.WindowPosition = value.ToString();
                Properties.Settings.Default.Save();
            }
        }

        public int TreeWidth
        {
            get
            {
                int width;
                if (!int.TryParse(Properties.Settings.Default.TreeWidth, out width))
                {
                    width = 300;
                }
                return width;
            }
            set
            {
                Properties.Settings.Default.TreeWidth = value.ToString();
                Properties.Settings.Default.Save();
            }
        }

        public IList<ConnectionInfo> Connections
        {
            get
            {
                return _connections.AsReadOnly();
            }
        }

        public void AddConnection(ConnectionInfo connectionInfo)
        {
            if (Properties.Settings.Default.Connections.Contains(connectionInfo.ToString()))
            {
                throw new ArgumentException(string.Format("A connection already exists for {0}", connectionInfo.ToString()), "connectionInfo");
            }
            _connections.Add(connectionInfo);
            Properties.Settings.Default.Connections.Add(connectionInfo.ToString());
            Properties.Settings.Default.Save();
            ConnectionChanged(this, new SettingsChangedEventArgs<ConnectionInfo>(SettingsChangedEventType.Added, connectionInfo));
        }

        private int FirstConnectionStringIndex(ConnectionInfo connectionInfo)
        {
            int idx = 0;
            foreach (var cnnStr in Properties.Settings.Default.Connections)
            {
                var compare = new ConnectionInfo(cnnStr);
                if (compare.Host == connectionInfo.Host
                    && compare.Port == connectionInfo.Port
                    && compare.ServiceId == connectionInfo.ServiceId
                    && compare.Username == connectionInfo.Username)
                {
                    return idx;
                }
                idx++;
            }
            return -1;
        }

        public void DeleteConnection(ConnectionInfo connectionInfo)
        {
            _connections.RemoveAll(
                cnn => 
                    cnn.Host == connectionInfo.Host &&
                    cnn.Port == connectionInfo.Port &&
                    cnn.ServiceId == connectionInfo.ServiceId);

            while (FirstConnectionStringIndex(connectionInfo) > -1)
            {
                Properties.Settings.Default.Connections.RemoveAt(FirstConnectionStringIndex(connectionInfo));
            }
            Properties.Settings.Default.Save();
            ConnectionChanged(this, new SettingsChangedEventArgs<ConnectionInfo>(SettingsChangedEventType.Deleted, connectionInfo));
        }

        private void LoadConnections()
        {
            _connections = new List<ConnectionInfo>();
            foreach (var cnnstr in Properties.Settings.Default.Connections)
            {
                _connections.Add(new ConnectionInfo(cnnstr));
            }
        }

        private void Init()
        {
            LoadConnections();
        }

        private static volatile SettingsManager _instance;
        private static object _sync = new object();

        private SettingsManager() 
        {
            Init();
        }

        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new SettingsManager();
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
