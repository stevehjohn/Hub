using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.DataObjects
{
    public enum IndexType
    {
        Ascending,
        Descending,
        Geospatial
    }

    public class IndexDescriptorProperty : INotifyPropertyChanged
    {
        private IndexType _indexType;

        public event PropertyChangedEventHandler PropertyChanged;

        public string PropertyName { get; set; }

        public IndexType IndexType 
        {
            get
            {
                return _indexType;
            }
            set
            {
                _indexType = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IndexType"));
                    PropertyChanged(this, new PropertyChangedEventArgs("IndexTypeDesc"));
                }
            }
        }

        public string IndexTypeDesc
        {
            get
            {
                switch (IndexType)
                {
                    case DataObjects.IndexType.Descending:
                        return Properties.Resources.IndexType_DescendingAbbr;
                    case DataObjects.IndexType.Geospatial:
                        return Properties.Resources.IndexType_GeospatialAbbr;
                    default:
                        return Properties.Resources.IndexType_AscendingAbbr;
                }
            }
        }
    }

    public class IndexDescriptor
    {
        public int Id { get; set; }

        public bool IsSparse { get; set; }

        public bool IsUnique { get; set; }

        public ObservableCollection<IndexDescriptorProperty> IndexedProperties { get; private set; }

        public IndexDescriptor()
        {
            IndexedProperties = new ObservableCollection<IndexDescriptorProperty>();
        }
    }
}
