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
using System.Collections.ObjectModel;

namespace MingControls.Controls
{
    public partial class LogViewer : UserControl
    {
        private ObservableCollection<LogEntry> _entries;
        private int _viewSize = 500;

        public LogViewer()
        {
            InitializeComponent();
            _entries = new ObservableCollection<LogEntry>();
            LogList.ItemsSource = _entries;
        }

        public void AddEntries(IEnumerable<LogEntry> entries)
        {
            entries.ToList().ForEach(entry => _entries.Insert(0, entry));
            while (_entries.Count > _viewSize)
            {
                _entries.RemoveAt(_entries.Count - 1);
            }
        }

        public void ReplaceEntries(IEnumerable<LogEntry> entries)
        {
            _entries.Clear();
            var count = 0;
            foreach (var entry in entries)
            {
                _entries.Add(entry);
                count++;
                if (count >= _viewSize)
                {
                    break;
                }
            }
        }

        public void ClearEntries()
        {
            _entries.Clear();
        }

        public int ViewSize
        {
            get
            {
                return _viewSize;
            }
            set
            {
                _viewSize = value;
            }
        }
    }
}
