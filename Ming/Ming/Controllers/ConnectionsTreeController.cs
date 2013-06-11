using Ming.Infrastructure;
using MingPluginInterfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ming.Controllers
{
    internal delegate void SelectedItemChangedEventHandler(MingTreeViewItem selectedItem);

    internal class ConnectionsTreeController
    {
        private readonly TreeView _treeView;

        public event SelectedItemChangedEventHandler SelectedItemChanged;

        public ConnectionsTreeController(TreeView connectionsTreeView)
        {
            _treeView = connectionsTreeView;
            _treeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(SelectionChanged);
            
            SettingsManager.Instance.ConnectionChanged += new SettingChangedEventHandler<ConnectionInfo>(Settings_ConnectionChanged);

            SettingsManager.Instance.Connections.ToList().ForEach(cnnInfo => DisplayConnection(cnnInfo));

            _treeView.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(Expanded));
            _treeView.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(Collapsed));
        }

        void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItemChanged(_treeView.SelectedItem as MingTreeViewItem);
        }

        public void RefreshSelectedNode()
        {
            LoadChildren((MingTreeViewItem) _treeView.SelectedItem);
        }

        public void RefreshNode(MingTreeViewItem node)
        {
            LoadChildren(node);
        }

        public void CopySelectedNode()
        {
            var node = GetSelectedNode();
            if (node != null)
            {
                var cnn = GetNodeConnection(node);
                var plugin = PluginManager.Instance.GetPluginInstance(cnn.ServiceId);
                plugin.CopyTreeviewNode(node, cnn);
            }
        }

        /*
        public MingTreeViewItem GetRootNode(MingTreeViewItem treeViewItem)
        {
            var climb = treeViewItem;
            while (climb.Parent != null)
            {
                climb = climb.Parent;
            }
            return climb;
        }*/

        public ConnectionInfo GetNodeConnection(MingTreeViewItem treeViewItem)
        {
            var climb = treeViewItem;
            while (!(climb.Data is IConnectionProvider))
            {
                climb = climb.Parent;
            }
            return (climb.Data as IConnectionProvider).ConnectionInfo;
        }

        public MingTreeViewItem GetSelectedNode()
        {
            return _treeView.SelectedItem as MingTreeViewItem;
        }

        private void Expanded(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;
            var mtvi = tvi.Header as MingTreeViewItem;

            LoadChildren(mtvi);
        }

        private void LoadChildren(MingTreeViewItem node)
        {
            MingApp.Instance.IndicateBusy(null);

            var cnn = GetNodeConnection(node);

            var plugin = PluginManager.Instance.GetPluginInstance(cnn.ServiceId);
            var loadtask = new CancelableTask<IEnumerable<MingTreeViewItem>>(
                () => plugin.TreeViewClient.NodeExpanded(node, cnn),
                result => TreeViewItemsLoaded(node, result));
            loadtask.Execute();
        }

        private void Collapsed(object sender, RoutedEventArgs e)
        {
            var mtvi = ((TreeViewItem)e.OriginalSource).Header as MingTreeViewItem;
            mtvi.Collapsed();
        }

        private void TreeViewItemsLoaded(MingTreeViewItem parent, IEnumerable<MingTreeViewItem> items)
        {
            parent.Items.Clear();
            if (items == null || items.Count() == 0)
            {
                parent.AddChild(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.warning), Properties.Resources.TreeView_NoData, null, false));
            }
            else
            {
                items.ToList().ForEach(item => parent.AddChild(item));
            }
            MingApp.Instance.IndicateIdle();
        }

        private void DisplayConnection(ConnectionInfo cnnInfo)
        {
            var img = Utilities.BitmapImageFromBitmap(PluginManager.Instance.GetPluginInstance(cnnInfo.ServiceId).TreeViewIcon);
            MingTreeViewItem cnnObj;
            if (!string.IsNullOrWhiteSpace(cnnInfo.Name))
            {
                cnnObj = new MingTreeViewItem(img, cnnInfo.Name, new TreeViewRootNodeData(cnnInfo), true);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(cnnInfo.Username))
                {
                    cnnObj = new MingTreeViewItem(img, string.Format("{0}@{1}:{2}", cnnInfo.Username, cnnInfo.Host, cnnInfo.Port), new TreeViewRootNodeData(cnnInfo), true);
                }
                else
                {
                    cnnObj = new MingTreeViewItem(img, string.Format("{0}:{1}", cnnInfo.Host, cnnInfo.Port), new TreeViewRootNodeData(cnnInfo), true);
                }
            }
            _treeView.Items.Add(cnnObj);
        }

        private void RemoveConnections(ConnectionInfo cnnInfo)
        {
            List<MingTreeViewItem> toRemove = new List<MingTreeViewItem>();
            foreach (var item in _treeView.Items)
            {
                var mtvi = item as MingTreeViewItem;
                if (mtvi != null)
                {
                    var root = mtvi.Data as TreeViewRootNodeData;
                    if (root != null)
                    {
                        var cnnItem = root.ConnectionInfo;
                        if (cnnItem.Host == cnnInfo.Host &&
                            cnnItem.Port == cnnInfo.Port &&
                            cnnItem.ServiceId == cnnInfo.ServiceId)
                        {
                            toRemove.Add(mtvi);
                        }
                    }
                }
            }
            toRemove.ForEach(item => _treeView.Items.Remove(item));
        }

        public void Settings_ConnectionChanged(object sender, SettingsChangedEventArgs<ConnectionInfo> args)
        {
            switch (args.EventType)
            {
                case SettingsChangedEventType.Added:
                    DisplayConnection(args.Setting);
                    break;
                case SettingsChangedEventType.Deleted:
                    RemoveConnections(args.Setting);
                    break;
            }
        }
    }
}
