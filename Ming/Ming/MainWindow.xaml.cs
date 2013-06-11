using Ming.Infrastructure;
using MingPluginInterfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ming
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            var mtvi = ((MenuItem)e.Source).DataContext as MingTreeViewItem;
            while (mtvi.Parent != null)
            {
                mtvi = mtvi.Parent;
            }
            MingApp.Instance.Disconnect(((TreeViewRootNodeData)mtvi.Data).ConnectionInfo);
        }

        private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var panel = sender as StackPanel;
            if (panel != null)
            {
                var items = MingApp.Instance.TreeViewContextMenu(panel.DataContext as MingTreeViewItem);
                if (items.Count == 0)
                    e.Handled = true;
                panel.ContextMenu.ItemsSource = items;
            }
        }

        public void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewitem = sender as TreeViewItem;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                treeViewitem.IsSelected = true;
                e.Handled = true;
            }
        }

        private void TabItemClose_Click(object sender, RoutedEventArgs e)
        {
            var id = (int)((Control)sender).Tag;
            MingApp.Instance.CloseDocumentTab(id);
        }

        private void TabTextMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                var id = (int)((Grid)sender).Tag;
                MingApp.Instance.CloseDocumentTab(id);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            MingApp.Instance.RemoveLongRunningOperation((sender as Control).Tag as OperationStatus);
        }

        private void ConnectionsTreePreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    MingApp.Instance.CopyRequestedOnTreeviewNode();
                    e.Handled = true;
                }
            }
            if (e.Key == Key.F5)
            {
                MingApp.Instance.RefreshCurrentTreeViewItem();
            }
        }

        private void DocumentsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabs = sender as TabControl;
            var tab = tabs.SelectedItem as TabDocument;
            if (tab != null)
            {
                MingApp.Instance.ActiveTabChanged(tab.Id);
            }
        }
    }
}
