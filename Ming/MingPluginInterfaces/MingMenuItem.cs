using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;

namespace MingPluginInterfaces
{
    public enum ContextMenuPosition
    {
        Default,
        AppendToEnd
    }

    [Flags]
    public enum MenuContext
    {
        MainMenu                = 1,
        TreeViewContext         = 1 << 1,
        ToolBarContext          = 1 << 2,
        TabDocumentControlled   = 1 << 3
    }

    public class MingMenuItem
    {
        public string Id { get; internal set; }
        public BitmapImage Icon { get; internal set; }
        public string Title { get; internal set; }
        public MenuContext MenuContext { get; internal set; }
        public bool IsSeparator { get; internal set; }
        public ContextMenuPosition ContextMenuPosition { get; internal set; }

        private List<MingMenuItem> _subMenuItems;

        public IEnumerable<MingMenuItem> SubMenuItems 
        { 
            get 
            {
                if (_subMenuItems == null)
                {
                    return Enumerable.Empty<MingMenuItem>();
                }
                return _subMenuItems.AsReadOnly(); 
            } 
        }
        internal IList<MingMenuItem> GetSubMenuItems { get { return _subMenuItems; } } 

        internal MingMenuItem() 
        {
            ContextMenuPosition = MingPluginInterfaces.ContextMenuPosition.Default;
        }

        public void AddSubMenuItem(MingMenuItem item)
        {
            if (_subMenuItems == null)
                _subMenuItems = new List<MingMenuItem>();

            _subMenuItems.Add(item);
        }
    }
}
