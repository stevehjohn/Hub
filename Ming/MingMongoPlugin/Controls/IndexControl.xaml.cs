using MingMongoPlugin.DataObjects;
using System;
using System.Collections;
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
    public delegate void IndexRemovedEventHandler(int id);

    public partial class IndexControl : UserControl
    {
        public event IndexRemovedEventHandler IndexRemoved;

        public static DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(int), typeof(IndexControl));

        public static DependencyProperty IndexedPropertiesProperty =
            DependencyProperty.Register("IndexedProperties", typeof(ObservableCollection<IndexDescriptorProperty>), typeof(IndexControl));

        public static DependencyProperty IsSparseProperty =
            DependencyProperty.Register("IsSparse", typeof(bool), typeof(IndexControl));

        public static DependencyProperty IsUniqueProperty =
            DependencyProperty.Register("IsUnique", typeof(bool), typeof(IndexControl));

        public IndexControl()
        {
            InitializeComponent();
        }

        private string _contextIndexName;

        public int Id
        {
            get
            {
                return (int)GetValue(IdProperty);
            }
            set
            {
                SetValue(IdProperty, value);
            }
        }

        public ObservableCollection<IndexDescriptorProperty> IndexedProperties
        {
            get
            {
                return (ObservableCollection<IndexDescriptorProperty>)GetValue(IndexedPropertiesProperty);
            }
            set
            {
                SetValue(IndexedPropertiesProperty, value);
            }
        }

        public bool IsSparse
        {
            get
            {
                return (bool)GetValue(IsSparseProperty);
            }
            set
            {
                SetValue(IsSparseProperty, value);
            }
        }

        public bool IsUnique
        {
            get
            {
                return (bool)GetValue(IsUniqueProperty);
            }
            set
            {
                SetValue(IsUniqueProperty, value);
            }
        }

        private void PropertyRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var src = e.Source as Button;
            if (src != null)
            {
                IndexedProperties.Remove(IndexedProperties.First(ip => ip.PropertyName == (string) src.Tag));
                if (IndexedProperties.Count == 0 && IndexRemoved != null)
                {
                    IndexRemoved(Id);
                }
            }
        }

        public void ShowContextMenu(object sender, MouseButtonEventArgs e)
        {
            var src = e.Source as FrameworkElement;
            if (src != null)
            {
                _contextIndexName = (string) src.Tag;
                var grid = src.Parent as Grid;
                if (grid != null)
                {
                    grid.ContextMenu.PlacementTarget = src;
                    grid.ContextMenu.IsOpen = true;
                }
            }
            e.Handled = true;
        }

        private void IndexTypeAscClicked(object sender, RoutedEventArgs e)
        {
            SetPropertyIndexType(IndexType.Ascending);
        }

        private void IndexTypeDescClicked(object sender, RoutedEventArgs e)
        {
            SetPropertyIndexType(IndexType.Descending);
        }
        
        private void IndexTypeGeoClicked(object sender, RoutedEventArgs e)
        {
            SetPropertyIndexType(IndexType.Geospatial);
        }

        private void SetPropertyIndexType(IndexType indexType)
        {
            IndexedProperties.First(ip => ip.PropertyName == _contextIndexName).IndexType = indexType;
        }

        private void PropertyListDrop(object sender, DragEventArgs e)
        {
            var property = (string) e.Data.GetData(DataFormats.StringFormat);

            IndexedProperties.Add(new IndexDescriptorProperty { PropertyName = property, IndexType = IndexType.Ascending });
        }
    }
}
