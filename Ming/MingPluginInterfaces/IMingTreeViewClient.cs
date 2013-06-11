using System.Collections.Generic;

namespace MingPluginInterfaces
{
    public interface IMingTreeViewClient : ITreeViewContextMenuClient
    {
        IEnumerable<MingTreeViewItem> NodeExpanded(MingTreeViewItem node, ConnectionInfo cnnInfo);
    }
}
