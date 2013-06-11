using MingMongoPlugin.DataObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MingMongoPlugin.Controls
{
    public partial class IndexListControl : UserControl
    {
        private int _nextId = 0;

        private ObservableCollection<IndexDescriptor> _indexes;

        public IndexListControl()
        {
            InitializeComponent();

            _indexes = new ObservableCollection<IndexDescriptor>();

            IndexList.ItemsSource = _indexes;
        }

        public IEnumerable<IndexDescriptor> Indexes
        {
            get
            {
                return _indexes;
            }
        }

        public void AddIndex(IndexDescriptor index)
        {
            index.Id = _nextId++;
            _indexes.Add(index);
        }

        private void IndexControlIndexRemoved(int id)
        {
            _indexes.Remove(_indexes.First(idx => idx.Id == id));
        }

        private void RectangleDrop(object sender, DragEventArgs e)
        {
            var property = (string)e.Data.GetData(DataFormats.StringFormat);

            var desc = new IndexDescriptor();
            desc.IndexedProperties.Add(new IndexDescriptorProperty { PropertyName = property });
            AddIndex(desc);
        }
    }
}
