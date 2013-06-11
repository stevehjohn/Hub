using System.Windows.Controls;

namespace MingPluginInterfaces
{
    public interface ITabDocument
    {
        UserControl Control { get; }

        string Title { get; }

        string Description { get; }

        bool CloseRequested();

        int InstanceId { set; }

        void MenuItemClicked(string menuKey);
    }
}
