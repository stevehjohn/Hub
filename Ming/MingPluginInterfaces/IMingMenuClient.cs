using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingPluginInterfaces
{
    public interface IMingMenuClient
    {
        void MenuItemHandler(ConnectionInfo cnnInfo, MingTreeViewItem node, string menuKey);

        IEnumerable<string> GetActiveMenuKeys(MingTreeViewItem activeNode);
    }
}
