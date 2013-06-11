using System;

namespace MingControls.Controls
{
    public class LogEntry : IEquatable<LogEntry>
    {
        private DateTime _time;
        private string _detail;
        private bool _isNew;

        public LogEntry(DateTime time, string detail)
        {
            _time = time;
            _detail = detail;
        }

        public DateTime Time { get { return _time; } }
        public string Detail { get { return _detail; } }

        public bool IsNew
        {
            get
            {
                return _isNew;
            }
            set
            {
                _isNew = value;
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Found this hashing algorithm on t'internet
                int hash = 17;
                hash = hash * 31 + _time.GetHashCode();
                hash = hash * 31 + _detail.GetHashCode();
                return hash;
            }
        }

        public bool Equals(LogEntry other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }
    }
}
