using System;
using System.Collections.Generic;

namespace MingPluginInterfaces
{
    public interface IMingPlugin
    {
        IMingApp MingApp { set; }

        string Id { get; }

        string Name { get; }

        string Description { get; }

        string Version { get; }

        int DefaultPort { get; }

        System.Drawing.Bitmap TreeViewIcon { get; }

        bool Test(ConnectionInfo cnn);

        IMingTreeViewClient TreeViewClient { get; }

        IMingMenuClient MenuClient { get; }

        IEnumerable<MingMenuItem> AllMenuItems { get; }

        void CopyTreeviewNode(MingTreeViewItem node, ConnectionInfo cnnInfo);
    }
}
