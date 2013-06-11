using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingPluginInterfaces
{
    public interface ITreeViewContextMenuClient
    {
        IEnumerable<string> GetActiveNodeContextMenuKeys(MingTreeViewItem node);
    }
}
