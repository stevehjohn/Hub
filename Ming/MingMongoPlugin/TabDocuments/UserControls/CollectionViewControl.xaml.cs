using MingPluginInterfaces;
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

namespace MingMongoPlugin.TabDocuments.UserControls
{
    public delegate void PropertyExpanderClickedEventHandler(MongoDocumentProperty documentProperty);
    public delegate void RefreshClickedEventHandler();
    public delegate void AddSortClickedEventHandler();
    public delegate void SortRemoveButtonClickedEventHandler(string sortProperty);
    public delegate void AddFilterClickedEventHandler();
    public delegate void FilterRemoveButtonClickedEventHandler(string sortProperty);

    public partial class CollectionViewControl : UserControl
    {
        public event PropertyExpanderClickedEventHandler PropertyExpanderClicked;
        public event RefreshClickedEventHandler RefreshClicked;
        public event AddSortClickedEventHandler AddSortClicked;
        public event SortRemoveButtonClickedEventHandler SortRemoveButtonClicked;
        public event AddFilterClickedEventHandler AddFilterClicked;
        public event FilterRemoveButtonClickedEventHandler FilterRemoveButtonClicked;

        public CollectionViewControl()
        {
            InitializeComponent();

            ShowSortPanel(Properties.Settings.Default.CollectionViewShowSort);
            ShowSort.IsChecked = Properties.Settings.Default.CollectionViewShowSort;
            ShowFilterPanel(Properties.Settings.Default.CollectionViewShowFilter);
            ShowFilter.IsChecked = Properties.Settings.Default.CollectionViewShowFilter;
        }

        public void PropertyExpander_Click(object sender, RoutedEventArgs e)
        {
            PropertyExpanderClicked(((Control)sender).Tag as MongoDocumentProperty);
        }

        private void PrevClick(object sender, RoutedEventArgs e)
        {
            var newValue = int.Parse(PageNumber.Text) - 1;
            PageNumber.SetValue(TextBox.TextProperty, newValue.ToString());
            BindingOperations.GetBindingExpression(PageNumber, TextBox.TextProperty).UpdateSource();
        }

        private void NextClick(object sender, RoutedEventArgs e)
        {
            var newValue = int.Parse(PageNumber.Text) + 1;
            PageNumber.SetValue(TextBox.TextProperty, newValue.ToString());
            BindingOperations.GetBindingExpression(PageNumber, TextBox.TextProperty).UpdateSource();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshClicked();
        }

        private void SortDirectionClick(object sender, RoutedEventArgs e)
        {
            if ((string)SortDirectionImage.Tag == "asc")
            {
                SortDirectionImage.Tag = "desc";
                SortDirectionImage.ToolTip = Properties.Resources.CollectionView_Desc;
                SortDirectionImage.Source = Utilities.BitmapImageFromBitmap(Properties.Resources.arrow_down);
            }
            else
            {
                SortDirectionImage.Tag = "asc";
                SortDirectionImage.ToolTip = Properties.Resources.CollectionView_Asc;
                SortDirectionImage.Source = Utilities.BitmapImageFromBitmap(Properties.Resources.arrow_up);
            }
        }

        private void ShowSortClick(object sender, RoutedEventArgs e)
        {
            ShowSortPanel((bool)ShowSort.IsChecked);

            Properties.Settings.Default.CollectionViewShowSort = (bool)ShowSort.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void ShowSortPanel(bool show)
        {
            if (show)
            {
                SortBorder.Visibility = System.Windows.Visibility.Visible;
                ShowSort.ToolTip = Properties.Resources.CollectionView_HideSort;
            }
            else
            {
                SortBorder.Visibility = System.Windows.Visibility.Collapsed;
                ShowSort.ToolTip = Properties.Resources.CollectionView_ShowSort;
            }
        }

        private void ShowFilterClick(object sender, RoutedEventArgs e)
        {
            ShowFilterPanel((bool)ShowFilter.IsChecked);

            Properties.Settings.Default.CollectionViewShowFilter = (bool)ShowFilter.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void ShowFilterPanel(bool show)
        {
            if (show)
            {
                FilterBorder.Visibility = System.Windows.Visibility.Visible;
                ShowFilter.ToolTip = Properties.Resources.CollectionView_HideFilter;
            }
            else
            {
                FilterBorder.Visibility = System.Windows.Visibility.Collapsed;
                ShowFilter.ToolTip = Properties.Resources.CollectionView_ShowFilter;
            }
        }

        private void AddSortClick(object sender, RoutedEventArgs e)
        {
            AddSortClicked();
        }

        private void SortRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var source = sender as Button;

            SortRemoveButtonClicked((string) source.Tag);
        }

        private void AddFilterClick(object sender, RoutedEventArgs e)
        {
            AddFilterClicked();
        }

        private void FilterRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var source = sender as Button;

            FilterRemoveButtonClicked((string)source.Tag);
        }
    }
}
