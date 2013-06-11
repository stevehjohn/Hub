using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MingPluginInterfaces;

namespace MingMongoPlugin
{
    internal static class Menus
    {
        public const string MenuStructure = "MONGO_MENUDATA";

        public const string NewDatabase = "MONGO_NEWDB";
        public const string DeleteDatabase = "MONGO_DELDB";
        public const string NewCollection = "MONGO_NEWCOL";
        public const string DeleteCollection = "MONGO_DELCOL";
        public const string ViewCollection = "MONGO_VIEWCOL";
        public const string CopyDocument = "MONGO_COPYDOC";
        public const string RenameCollection = "MONGO_RENAMECOL";
        public const string EvaluateJavaScript = "MONGO_EVALJS";
        public const string CopyCollection = "MONGO_COPYCOL";
        public const string CopyDatabase = "MONGO_COPYDB";
        public const string RenameDatabase = "MONGO_RENAMEDB";
        public const string DeleteDocument = "MONGO_DELDOC";
        // Compact collection?
        public const string CompactCollections = "MONGO_COMPACTCOLS";
        public const string WatchLogs = "MONGO_WATCHLOGS";
        public const string CopyCollections = "MONGO_COPYCOLS";
        // TODO: Retire copy collection/copy database
        public const string OpenConsole = "MONGO_CONSOLE";
        public const string ManageIndexes = "MONGO_INDEXES";
        public const string SystemStatus = "MONGO_SYSSTAT";
        public const string DeleteColsDBs = "MONGO_DELCOLDB";

        public static IEnumerable<MingMenuItem> GetMenus()
        {
            List<MingMenuItem> items = new List<MingMenuItem>();

            var menu = MingMenuItemFactory.CreateMenuItem(MenuStructure, null, Properties.Resources.Menu_Data, MenuContext.MainMenu);
            items.Add(menu);
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(ViewCollection, Properties.Resources.view, Properties.Resources.MenuItem_ViewData, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TreeViewContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(RenameCollection, null, Properties.Resources.MenuItem_RenameCollection, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TreeViewContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(CopyCollections, Properties.Resources.database_collection, Properties.Resources.MenuItem_CopyCollections, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            //menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(CopyCollection, Properties.Resources.collection_copy, Properties.Resources.MenuItem_CopyCollection, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(NewCollection, Properties.Resources.collection_add, Properties.Resources.MenuItem_AddCollection, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(DeleteCollection, Properties.Resources.collection_delete, Properties.Resources.MenuItem_DeleteCollection, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(ManageIndexes, Properties.Resources.indexes, Properties.Resources.MenuItem_ManageIndexes, MenuContext.TreeViewContext | MenuContext.ToolBarContext | MenuContext.MainMenu));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateSeparator(MenuContext.MainMenu));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(WatchLogs, Properties.Resources.log, Properties.Resources.MenuItem_WatchLogs, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TreeViewContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateSeparator(MenuContext.MainMenu));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(DeleteDocument, Properties.Resources.document_delete, Properties.Resources.MenuItem_DeleteDocument, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TabDocumentControlled));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateSeparator(MenuContext.MainMenu));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(OpenConsole, Properties.Resources.console, Properties.Resources.MenuItem_OpenConsole, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TreeViewContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(SystemStatus, Properties.Resources.server_status, Properties.Resources.MenuItem_SystemStatus, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TreeViewContext));
            //menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(EvaluateJavaScript, Properties.Resources.script, Properties.Resources.MenuItem_EvalJS, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TreeViewContext));
            //menu.AddSubMenuItem(MingMenuItemFactory.CreateSeparator(MenuContext.MainMenu));
            //menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(CopyDocument, Properties.Resources.documents, Properties.Resources.MenuItem_CopyDocument, MenuContext.MainMenu | MenuContext.ToolBarContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateSeparator(MenuContext.MainMenu));
            //menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(RenameDatabase, null, Properties.Resources.MenuItem_RenameDatabase, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            //menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(CopyDatabase, Properties.Resources.database_copy, Properties.Resources.MenuItem_CopyDatabase, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(NewDatabase, Properties.Resources.database_add, Properties.Resources.MenuItem_AddDatabase, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(DeleteDatabase, Properties.Resources.database_delete, Properties.Resources.MenuItem_DeleteDatabase, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(CompactCollections, Properties.Resources.collections_compact, Properties.Resources.MenuItem_CompactCollections, MenuContext.TreeViewContext | MenuContext.MainMenu | MenuContext.ToolBarContext));
            //menu.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(DeleteColsDBs, null, Properties.Resources.MenuItem_DeleteColllectionsDBs, MenuContext.TreeViewContext | MenuContext.MainMenu));

            return items;
        }
    }
}
