using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using MingPluginInterfaces;

namespace Ming.Infrastructure
{
    internal delegate void ActiveTabMenuItemClicked(string menuKey);

    internal class MenuController
    {
        public event ActiveTabMenuItemClicked ActiveTabMenuItemClicked;

        private readonly Menu _menu;
        private readonly ToolBarTray _toolBarTray;
        private IList<MingMenuItemAndHandler> _menuItems;
        private IList<object> _contextMenuItems;
        private IList<ITreeViewContextMenuClient> _contextMenuKeyProviders;
        private IList<object> _contextMenuAppendToEndItems;
        private MingTreeViewItem _contextMenuOriginator;
        private IList<IMingMenuClient> _menuClients;
        private IDictionary<string, IList<string>> _pluginTabDocumentItems;

        public MenuController(Menu menu, ToolBarTray toolBarTray)
        {
            _menu = menu;
            _toolBarTray = toolBarTray;
            _menuItems = new List<MingMenuItemAndHandler>();
            _contextMenuItems = new List<object>();
            _contextMenuKeyProviders = new List<ITreeViewContextMenuClient>();
            _menuClients = new List<IMingMenuClient>();
            _pluginTabDocumentItems = new Dictionary<string, IList<string>>();
        }

        public void AddContextMenuKeyProvider(ITreeViewContextMenuClient provider)
        {
            _contextMenuKeyProviders.Add(provider);
        }

        public void AddMenuItems(IEnumerable<MingMenuItem> items, IMingMenuClient menuClient, string pluginId)
        {
            items.ToList().ForEach(item => _menuItems.Add(
                new MingMenuItemAndHandler(item, menuClient, pluginId)));
            _menuClients.Add(menuClient);
        }

        public void CreateMenus()
        {
            _contextMenuAppendToEndItems = new List<object>();
            CreateSubMenus(_menu.Items, _menuItems, null, 0);
            _contextMenuItems = _contextMenuItems.Concat(_contextMenuAppendToEndItems).ToList();
            _contextMenuAppendToEndItems = null;
        }

        private void CreateSubMenus(ItemCollection parent, IEnumerable<MingMenuItemAndHandler> items, List<MingMenuItemAndHandler> pToolItems, int level)
        {
            List<MingMenuItemAndHandler> toolItems;
            if (level == 1)
            {
                toolItems = new List<MingMenuItemAndHandler>();
            }
            else
            {
                toolItems = pToolItems;
            }
            foreach (var itemAndHandler in items)
            {
                var item = itemAndHandler.MenuItem;
                if (item.IsSeparator)
                {
                    if ((item.MenuContext & MenuContext.MainMenu) == MenuContext.MainMenu)
                    {
                        parent.Add(new Separator());
                    }
                    if ((item.MenuContext & MenuContext.TreeViewContext) == MenuContext.TreeViewContext)
                    {
                        if (item.ContextMenuPosition == ContextMenuPosition.AppendToEnd)
                        {
                            _contextMenuAppendToEndItems.Add(new Separator());
                        }
                        else
                        {
                            _contextMenuItems.Add(new Separator());
                        }
                    }
                }
                else
                {
                    if ((item.MenuContext & MenuContext.TabDocumentControlled) == MenuContext.TabDocumentControlled)
                    {
                        if ((item.MenuContext & MenuContext.TreeViewContext) == MenuContext.TreeViewContext)
                        {
                            throw new ArgumentException(string.Format("Menu item cannot be TreeViewContext and TabDocumentControlled. Culprit: {0}", item.Id));
                        }
                        if (!_pluginTabDocumentItems.ContainsKey(itemAndHandler.PluginId))
                        {
                            _pluginTabDocumentItems.Add(itemAndHandler.PluginId, new List<string>());
                        }
                        _pluginTabDocumentItems[itemAndHandler.PluginId].Add(item.Id);
                    }

                    if ((item.MenuContext & MenuContext.ToolBarContext) == MenuContext.ToolBarContext && level > 0)
                    {
                        if (item.Icon != null)
                        {
                            toolItems.Add(itemAndHandler);
                        }
                    }

                    if ((item.MenuContext & MenuContext.TreeViewContext) == MenuContext.TreeViewContext)
                    {
                        var menuItem = new MenuItem();

                        menuItem.Header = item.Title;
                        if (item.Icon != null)
                        {
                            menuItem.Icon = new Image() { Source = item.Icon };
                        }
                        menuItem.Name = item.Id;
                        menuItem.Click += new System.Windows.RoutedEventHandler(MenuItem_Click);
                        menuItem.Tag = itemAndHandler; //.MenuClient;

                        if (item.ContextMenuPosition == ContextMenuPosition.AppendToEnd)
                        {
                            _contextMenuAppendToEndItems.Add(menuItem);
                        }
                        else
                        {
                            _contextMenuItems.Add(menuItem);
                        }
                    }

                    if ((item.MenuContext & MenuContext.MainMenu) == MenuContext.MainMenu)
                    {
                        var menuItem = new MenuItem();

                        if (level == 0)
                        {
                            menuItem.Header = item.Title.ToUpper();
                        }
                        else
                        {
                            menuItem.Header = item.Title;
                        }
                        if (item.Icon != null)
                        {
                            menuItem.Icon = new Image() { Source = item.Icon };
                        }
                        menuItem.Name = item.Id;
                        menuItem.Tag = itemAndHandler; //.MenuClient;
                        if (level > 0)
                        {
                            menuItem.Opacity = 0.4;
                            menuItem.IsEnabled = false;
                        }
                        if (level > 0)
                        {
                            menuItem.Click += new System.Windows.RoutedEventHandler(MenuItem_Click);
                        }
                        else
                        {
                            menuItem.SubmenuOpened += new System.Windows.RoutedEventHandler(MainMenu_Click);
                        }

                        parent.Add(menuItem);

                        CreateSubMenus(menuItem.Items, item.SubMenuItems.Select(sel => new MingMenuItemAndHandler(sel, itemAndHandler.MenuClient, itemAndHandler.PluginId)), toolItems, level + 1);
                    }
                }
            }
            if (level == 1 && toolItems.Count > 0)
            {
                var toolBar = new ToolBar();
                toolItems.ForEach(item => 
                    {
                        var button = new Button
                        {
                            ToolTip = item.MenuItem.Title.Replace("_", ""),
                            Content = new Image() { Source = item.MenuItem.Icon },
                            Tag = item, //.MenuClient,
                            Name = item.MenuItem.Id
                        };
                        button.Click += new System.Windows.RoutedEventHandler(MenuItem_Click);
                        toolBar.Items.Add(button);
                    });
                _toolBarTray.ToolBars.Add(toolBar);
            }
        }

        public void ActivateToolBarItems()
        {
            IEnumerable<string> activeKeys = new List<string>();
            _menuClients.ToList().ForEach(item => activeKeys = activeKeys.Union(item.GetActiveMenuKeys(MingApp.Instance.GetCurrentTreeViewItem())));

            foreach (var bar in _toolBarTray.ToolBars)
            {
                foreach (var item in bar.Items)
                {
                    var button = item as Button;
                    if (button != null)
                    {
                        if (activeKeys.Contains(button.Name))
                        {
                            button.Opacity = 1.0;
                            button.IsEnabled = true;
                        }
                        else
                        {
                            button.Opacity = 0.4;
                            button.IsEnabled = false;
                        }
                    }
                }
            }
        }

        public void MainMenu_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var control = sender as MenuItem;
            if (control != null)
            {
                var menuHandler = control.Tag as MingMenuItemAndHandler;
                IEnumerable<string> activeKeys = Enumerable.Empty<string>();
                if (menuHandler != null)
                {
                    activeKeys = menuHandler.MenuClient.GetActiveMenuKeys(MingApp.Instance.GetCurrentTreeViewItem());
                }
                ActivateMenuItems(control.Items, activeKeys);
            }
        }

        private void ActivateMenuItems(ItemCollection items, IEnumerable<string> activeKeys)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                var menuItem = item as MenuItem;
                if (menuItem != null)
                {
                    var itemAndHandler = menuItem.Tag as MingMenuItemAndHandler;
                    if ((itemAndHandler.MenuItem.MenuContext & MenuContext.TabDocumentControlled) != MenuContext.TabDocumentControlled)
                    {
                        if (activeKeys.Contains(menuItem.Name))
                        {
                            menuItem.Opacity = 1.0;
                            menuItem.IsEnabled = true;
                        }
                        else
                        {
                            menuItem.Opacity = 0.4;
                            menuItem.IsEnabled = false;
                        }
                    }

                    ActivateMenuItems(menuItem.Items, activeKeys);
                }
            }
        }

        public void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control != null)
            {
                var menuHandler = control.Tag as MingMenuItemAndHandler;
                if (menuHandler != null)
                {
                    var treeNode = MingApp.Instance.GetCurrentTreeViewItem();
                    if (_pluginTabDocumentItems.Any(pluginList => pluginList.Value.Contains(control.Name)))
                    {
                        if (ActiveTabMenuItemClicked != null) ActiveTabMenuItemClicked(control.Name);
                    }
                    else
                    {
                        menuHandler.MenuClient.MenuItemHandler(MingApp.Instance.GetTreeViewNodeConnectionInfo(treeNode), treeNode, control.Name);
                    }
                }
            }
        }

        public IList<object> GetContextMenu(MingTreeViewItem originator, PluginInfo plugin)
        {
            _contextMenuOriginator = originator;

            var keys = plugin.Instance.TreeViewClient.GetActiveNodeContextMenuKeys(originator);

            _contextMenuKeyProviders.ToList().ForEach(item => keys = keys.Union(item.GetActiveNodeContextMenuKeys(originator)));

            var items = _contextMenuItems.Where(item => item is MenuItem ? keys.Contains(((MenuItem) item).Name) : true).ToList();

            if (items.Count > 0)
            {
                if (items[0] is Separator)
                {
                    items.RemoveAt(0);
                }
                if (items.Count > 0)
                {
                    if (items.Last() is Separator)
                    {
                        items.RemoveAt(items.Count - 1);
                    }
                }
            }

            return items;
        }

        private void DeactivateAllPluginToolbarButtons()
        {
            foreach (var bar in _toolBarTray.ToolBars)
            {
                foreach (var item in bar.Items)
                {
                    var button = item as Button;
                    if (button != null)
                    {
                        if (_pluginTabDocumentItems.Any(pluginList => pluginList.Value.Contains(button.Name)))
                        {
                            button.Opacity = 0.4;
                            button.IsEnabled = false;
                        }
                    }
                }
            }
        }

        private void DeactivateAllPluginTabDocumentMainMenuItems(ItemCollection items)
        {
            foreach (var menuObj in items)
            {
                var menuItem = menuObj as MenuItem;
                if (menuItem != null)
                {
                    if (_pluginTabDocumentItems.Any(pluginList => pluginList.Value.Contains(menuItem.Name)))
                    {
                        menuItem.Opacity = 0.4;
                        menuItem.IsEnabled = false;
                    }
                    DeactivateAllPluginTabDocumentMainMenuItems(menuItem.Items);
                }
            }
        }

        public void DeactivateAllPluginTabDocumentMenus()
        {
            DeactivateAllPluginToolbarButtons();
            DeactivateAllPluginTabDocumentMainMenuItems(_menu.Items);
        }

        private void ActivateTabDocumentToolbarButtons(IEnumerable<string> activeMenuKeys)
        {
            foreach (var bar in _toolBarTray.ToolBars)
            {
                foreach (var item in bar.Items)
                {
                    var button = item as Button;
                    if (button != null)
                    {
                        if (activeMenuKeys.Contains(button.Name))
                        {
                            button.Opacity = 1.0;
                            button.IsEnabled = true;
                        }
                    }
                }
            }
        }

        private void ActivateTabDocumentMainMenuItems(ItemCollection items, IEnumerable<string> activeMenuKeys)
        {
            foreach (var menuObj in items)
            {
                var menuItem = menuObj as MenuItem;
                if (menuItem != null)
                {
                    if (activeMenuKeys.Contains(menuItem.Name))
                    {
                        menuItem.Opacity = 1.0;
                        menuItem.IsEnabled = true;
                    }
                    ActivateTabDocumentMainMenuItems(menuItem.Items, activeMenuKeys);
                }
            }
        }

        public void ActivateTabDocumentMenuItems(IEnumerable<string> activeMenuKeys)
        {
            DeactivateAllPluginTabDocumentMenus();
            /*
            if (!_pluginTabDocumentItems.Any(pluginList => pluginList.Value.Contains(activeMenuKeys)))
            {
                throw new ArgumentException(string.Format("Menu item {0} is not TabDocumentControlled"));
            }*/
            ActivateTabDocumentToolbarButtons(activeMenuKeys);
            ActivateTabDocumentMainMenuItems(_menu.Items, activeMenuKeys);
        }
    }
}
