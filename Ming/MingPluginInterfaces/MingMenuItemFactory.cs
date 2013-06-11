using System.Drawing;

namespace MingPluginInterfaces
{
    public class MingMenuItemFactory
    {
        public static MingMenuItem CreateMenuItem(string id, Bitmap icon, string title, MenuContext menuContext)
        {
            var item = new MingMenuItem();

            item.Id = id;
            item.Icon = Utilities.BitmapImageFromBitmap(icon);
            item.Title = title;
            item.MenuContext = menuContext;

            return item;
        }

        public static MingMenuItem CreateMenuItem(string id, Bitmap icon, string title, MenuContext menuContext, ContextMenuPosition position)
        {
            var item = CreateMenuItem(id, icon, title, menuContext);
            item.ContextMenuPosition = position;

            return item;
        }

        public static MingMenuItem CreateSeparator(MenuContext menuContext)
        {
            var item = new MingMenuItem();

            item.IsSeparator = true;
            item.MenuContext = menuContext;

            return item;
        }

        public static MingMenuItem CreateSeparator(MenuContext menuContext, ContextMenuPosition position)
        {
            var item = CreateSeparator(menuContext);
            item.ContextMenuPosition = position;

            return item;
        }
    }
}
