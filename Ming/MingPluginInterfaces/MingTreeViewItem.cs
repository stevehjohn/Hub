using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace MingPluginInterfaces
{
    public class MingTreeViewItem
    {
        private readonly BitmapImage _icon;
        private readonly string _text;
        private readonly string _secondaryText;
        private readonly object _data;
        private readonly ObservableCollection<object> _items;
        private readonly bool _hasDynamicChildren;

        private MingTreeViewItem _parent;

        public MingTreeViewItem(BitmapImage icon, string text, object data, bool hasDynamicChildren)
        {
            _icon = icon;
            _text = text;
            _secondaryText = null;
            _data = data;
            _items = new ObservableCollection<object>();
            _hasDynamicChildren = hasDynamicChildren;
            if (hasDynamicChildren)
            {
                AddChild(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.refresh), Properties.Resources.TreeView_Loading, null, false));
            }
        }

        public MingTreeViewItem(BitmapImage icon, string text, string secondaryText, object data, bool hasDynamicChildren)
        {
            _icon = icon;
            _text = text;
            _secondaryText = secondaryText;
            _data = data;
            _items = new ObservableCollection<object>();
            _hasDynamicChildren = hasDynamicChildren;
            if (hasDynamicChildren)
            {
                AddChild(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.refresh), Properties.Resources.TreeView_Loading, null, false));
            }
        }

        public void AddChild(MingTreeViewItem child)
        {
            child._parent = this;
            _items.Add(child);
        }

        public void Collapsed()
        {
            if (_hasDynamicChildren)
            {
                _items.Clear();
                AddChild(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.refresh), Properties.Resources.TreeView_Loading, null, false));
            }
        }

        public BitmapImage Icon { get { return _icon; } }
        public string Text { get { return _text; } }
        public string SecondaryText { get { return _secondaryText; } }
        public object Data { get { return _data; } }
        public ObservableCollection<object> Items { get { return _items; } }
        public MingTreeViewItem Parent { get { return _parent; } }
        public bool HasDynamicChildren { get { return _hasDynamicChildren; } }
    }
}
