using MingPluginInterfaces;

namespace Ming.Infrastructure
{
    internal class MingMenuItemAndHandler
    {
        public string PluginId { get; private set; }
        public MingMenuItem MenuItem { get; private set; }
        public IMingMenuClient MenuClient { get; private set; }

        public MingMenuItemAndHandler(MingMenuItem menuItem, IMingMenuClient menuClient, string pluginId)
        {
            MenuItem = menuItem;
            MenuClient = menuClient;
            PluginId = pluginId;
        }
    }
}
