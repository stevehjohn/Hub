using MongoDB.Bson;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MingMongoPlugin.TabDocuments
{
    public class MongoDocumentProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IList<MongoDocumentProperty> _children;

        private bool _isInError;

        private bool _isUpdating;

        private string _value;
        
        private Visibility _visible;

        private bool _wasUpdated;

        public bool AlternateRow { get; set; }

        public int? ArrayIndex { get; set; }

        public IEnumerable<MongoDocumentProperty> Children { get { return _children; } }

        public int Depth { get; set; }

        public BsonValue DocumentObjectId { get; set; }

        public bool Expanded { get; set; }

        public Visibility ExpanderVisibility { get; set; }

        public string FullPath { get; set; }

        public int Id { get; set; }

        public bool IsInError 
        {
            get
            {
                return _isInError;
            }
            set
            {
                _isInError = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsInError"));
            }
        }

        public bool IsUpdating
        {
            get
            {
                return _isUpdating;
            }
            set
            {
                _isUpdating = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsUpdating"));
            }
        }

        public string Key { get; set; }

        public MongoDocumentProperty Parent { get; set; }

        public bool ReadOnly { get; set; }

        public void SetInitialValue(string value)
        {
            _value = value;
        }

        public Visibility TextBoxVisibility { get; set; }

        public BsonType Type { get; set; }

        public void Updated()
        {
            IsUpdating = false;
            _wasUpdated = true;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("WasUpdated"));
            _wasUpdated = false;
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("WasUpdated"));
        }

        public string Value 
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Value"));
            }
        }

        public Visibility Visible 
        { 
            get 
            { 
                return _visible; 
            } 
            set
            { 
                _visible = value; 
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Visible")); 
            } 
        }

        public bool WasUpdated
        {
            get
            {
                return _wasUpdated;
            }
        }

        public MongoDocumentProperty()
        {
            _children = new List<MongoDocumentProperty>();
        }

        public void AddChild(MongoDocumentProperty child)
        {
            child.Parent = this;
            _children.Add(child);
        }
    }
}
