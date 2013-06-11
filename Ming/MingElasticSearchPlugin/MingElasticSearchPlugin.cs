using MingPluginInterfaces;
using System;

namespace MingElasticSearchPlugin
{
    public class MingElasticSearchPlugin : IMingPlugin
    {
        public string Id
        {
            get { return "elasticsearch"; }
        }

        public string Name
        {
            get { return Properties.Resources.PluginTitle; }
        }

        public int DefaultPort
        {
            get { return 9200; }
        }


        public System.Drawing.Bitmap TreeViewIcon
        {
            get { return Properties.Resources.elastic_treeview; }
        }

        public bool Test(ConnectionInfo cnn)
        {
            System.Threading.Thread.Sleep(1000);
            return false;
        }

        public IMingTreeViewClient TreeViewClient
        {
            get { return null; }
        }

        public System.Collections.Generic.IEnumerable<MingMenuItem> AllMenuItems
        {
            get
            {
                return System.Linq.Enumerable.Empty<MingMenuItem>();
            }
        }


        public System.Collections.Generic.IEnumerable<string> GetActiveNodeContextMenuKeys(object nodeData)
        {
            throw new NotImplementedException();
        }

        public void MenuItemHandler(ConnectionInfo cnnInfo, MingTreeViewItem node, string menuKey)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<string> GetActiveMenuKeys(MingTreeViewItem activeNode)
        {
            return System.Linq.Enumerable.Empty<string>();
        }

        public IMingMenuClient MenuClient
        {
            get { return null; }
        }
    }
}
