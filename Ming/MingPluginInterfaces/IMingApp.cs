using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace MingPluginInterfaces
{
    public interface IMingApp
    {
        void AddDocumentTab(ITabDocument document, string pluginId);

        void IndicateBusy(string message);

        void IndicateIdle();

        Window MainWindow { get; }

        void RefreshTreeViewItem(MingTreeViewItem node);

        void AddLongRunningOperation(OperationStatus status);

        void SetMenuItemStateForTab(string menuKey, bool enabled, int tabInstanceId);

        IEnumerable<ConnectionInfo> GetConnectionsForPlugin(string pluginId);
    }
}
