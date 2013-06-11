using System.Collections.Generic;
using System.Linq;
using MingPluginInterfaces;
using MingMongoPlugin.TreeViewObjects;
using MongoDB.Driver;
using MongoDB.Bson;
using MingMongoPlugin.Mongo;

namespace MingMongoPlugin
{
    internal class MingTreeViewClient : IMingTreeViewClient
    {
        private const string SystemCollectionPrefix = "system.";

        public IEnumerable<MingTreeViewItem> NodeExpanded(MingTreeViewItem node, ConnectionInfo cnnInfo)
        {
            var nodeData = node.Data;

            if (nodeData is TreeViewRootNodeData)
            {
                return LoadReplicaSet(cnnInfo);
            }
            if (nodeData is ReplicaSetMemberNode)
            {
                return LoadDatabases(cnnInfo);
            }
            if (nodeData is DatabaseNode)
            {
                return GetDatabaseObjectNodes(nodeData as DatabaseNode);
            }
            if (nodeData is CollectionsNode)
            {
                return LoadCollections(nodeData as CollectionsNode, cnnInfo);
            }
            if (nodeData is CollectionNode)
            {
                return GetCollectionObjectNodes(nodeData as CollectionNode);
            }
            if (nodeData is CollectionIndexesNode)
            {
                return LoadCollectionIndexes(nodeData as CollectionIndexesNode, cnnInfo);
            }
            if (nodeData is CollectionPropertiesNode)
            {
                return LoadCollectionProperties(nodeData as CollectionPropertiesNode, cnnInfo);
            }
            if (nodeData is CollectionPropertyNode)
            {
                return LoadPropertyProperties(nodeData as CollectionPropertyNode, cnnInfo);
            }

            return null;
        }

        private IEnumerable<MingTreeViewItem> LoadReplicaSet(ConnectionInfo cnnInfo)
        {
            try
            {
                var rsState = MongoUtilities.GetReplicaSetInfo(cnnInfo);
                if (rsState == null)
                {
                    return LoadDatabases(cnnInfo);
                }

                var items = new List<MingTreeViewItem>();

                rsState.ToList().ForEach(member =>
                    {
                        var dynamicChildren = !member.IsArbiter;
                        var image = member.IsPrimary ? Properties.Resources.primary : member.IsArbiter ? Properties.Resources.arbiter : Properties.Resources.secondary;
                        items.Add(
                            new MingTreeViewItem(
                                Utilities.BitmapImageFromBitmap(image),
                                    string.Format("{0}:{1}", member.Host, member.Port),
                                    new ReplicaSetMemberNode(member.Host, member.Port, member.IsPrimary),
                                    dynamicChildren));
                    });

                return items;
            }
            catch
            {
                return ReturnConnectionFailure();
            }
        }

        private IEnumerable<MingTreeViewItem> LoadDatabases(ConnectionInfo cnnInfo)
        {
            IEnumerable<string> databases;
            try
            {
                databases = MongoUtilities.Create(cnnInfo).GetDatabaseNames();
            }
            catch
            {
                return ReturnConnectionFailure();
            }

            if (databases.Count() == 0)
            {
                return new List<MingTreeViewItem> 
                    { new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.warning), Properties.Resources.TreeView_NoDatabases, new DatabasesEmptyNode(), false) };
            }
            
            var nodes = new List<MingTreeViewItem>();
            databases.ToList().ForEach(db => 
                nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.database), db, new DatabaseNode(db), true)));

            return nodes;
        }

        private IEnumerable<MingTreeViewItem> GetDatabaseObjectNodes(DatabaseNode parent)
        {
            var nodes = new List<MingTreeViewItem>();

            nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.collections), Properties.Resources.TreeView_Collections, 
                new CollectionsNode(parent.DatabaseName), true));

            return nodes;
        }

        private IEnumerable<MingTreeViewItem> LoadCollections(CollectionsNode parent, ConnectionInfo cnnInfo)
        {
            IEnumerable<string> collections;
            try
            {
                collections = MongoUtilities.Create(cnnInfo).GetDatabase(parent.DatabaseName).GetCollectionNames();
            }
            catch
            {
                return ReturnConnectionFailure();
            }

            if (collections.Count() == 0)
            {
                return new List<MingTreeViewItem> 
                    { new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.warning), Properties.Resources.TreeView_NoCollections, new CollectionsEmptyNode(), false) };
            }

            var nodes = new List<MingTreeViewItem>();
            collections.Where(col => ! col.StartsWith(SystemCollectionPrefix)).ToList().ForEach(col =>
                nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.collection), col, 
                    new CollectionNode(parent.DatabaseName, col), true)));

            return nodes;
        }

        private IEnumerable<MingTreeViewItem> GetCollectionObjectNodes(CollectionNode parent)
        {
            var nodes = new List<MingTreeViewItem>();

            nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.documents), Properties.Resources.TreeView_Properties,
                new CollectionPropertiesNode(parent.DatabaseName, parent.CollectionName), true));
            nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.indexes), Properties.Resources.TreeView_Indexes,
                new CollectionIndexesNode(parent.DatabaseName, parent.CollectionName), true));

            return nodes;
        }

        private IEnumerable<MingTreeViewItem> LoadCollectionIndexes(CollectionIndexesNode parent, ConnectionInfo cnnInfo)
        {
            GetIndexesResult indexes;
            try
            {
                indexes = MongoUtilities.Create(cnnInfo).GetDatabase(parent.DatabaseName).GetCollection(parent.CollectionName).GetIndexes();
            }
            catch
            {
                return ReturnConnectionFailure();
            }

            if (indexes.Count() == 0)
            {
                return new List<MingTreeViewItem> 
                    { new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.warning), Properties.Resources.TreeView_NoIndexes, new CollectionIndexesEmptyNode(), false) };
            }

            var nodes = new List<MingTreeViewItem>();
            indexes.ToList().ForEach(idx =>
                nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.index), idx.Name,
                    new CollectionIndexNode(parent.DatabaseName, parent.CollectionName, idx.Name), false)));

            return nodes;
        }

        private IEnumerable<MingTreeViewItem> LoadCollectionProperties(CollectionPropertiesNode parent, ConnectionInfo cnnInfo)
        {
            IEnumerable<CollectionProperty> properties;
            try
            {
                var analyser = new CollectionPropertyAnalyser(MongoUtilities.Create(cnnInfo), parent.DatabaseName, parent.CollectionName);

                properties = analyser.GetProperties(null);
            }
            catch
            {
                return ReturnConnectionFailure();
            }

            var nodes = new List<MingTreeViewItem>();
            properties.ToList().ForEach(property => 
                {
                    var hasChildren = property.Type == BsonType.Document || property.Type == BsonType.Array;
                    nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(hasChildren ? Properties.Resources.documents : Properties.Resources.document), 
                        property.Name,
                        string.Format("({0})", property.Type.ToString().ToLower()),
                        new CollectionPropertyNode(parent.DatabaseName, parent.CollectionName, property.Name, property.Name), hasChildren));
                });

            return nodes;
        }

        private IEnumerable<MingTreeViewItem> LoadPropertyProperties(CollectionPropertyNode parent, ConnectionInfo cnnInfo)
        {
            IEnumerable<CollectionProperty> properties;
            try
            {
                var analyser = new CollectionPropertyAnalyser(MongoUtilities.Create(cnnInfo), parent.DatabaseName, parent.CollectionName);

                var parents = new List<string>();

                properties = analyser.GetProperties(parent.FullPath);
            }
            catch
            {
                return ReturnConnectionFailure();
            }

            var nodes = new List<MingTreeViewItem>();
            properties.ToList().ForEach(property =>
            {
                var hasChildren = property.Type == BsonType.Document || property.Type == BsonType.Array;
                nodes.Add(new MingTreeViewItem(Utilities.BitmapImageFromBitmap(hasChildren ? Properties.Resources.documents : Properties.Resources.document),
                    property.Name,
                    string.Format("({0})", property.Type.ToString().ToLower()),
                    new CollectionPropertyNode(parent.DatabaseName, parent.CollectionName, property.Name, string.Format("{0}.{1}", parent.FullPath, property.Name)), hasChildren));
            });
            return nodes;
        }

        private IEnumerable<MingTreeViewItem> ReturnConnectionFailure()
        {
            return new List<MingTreeViewItem> 
                { new MingTreeViewItem(Utilities.BitmapImageFromBitmap(Properties.Resources.error), Properties.Resources.TreeView_ConnectionFailure, null, false) };
        }

        public IEnumerable<string> GetActiveNodeContextMenuKeys(MingTreeViewItem node)
        {
            var keys = new List<string>();

            var nodeData = node.Data;

            if (nodeData is DatabaseNode || nodeData is CollectionsNode || nodeData is CollectionsEmptyNode)
            {
                keys.Add(Menus.NewCollection);
                if (!(nodeData is CollectionIndexesEmptyNode))
                {
                    keys.Add(Menus.CompactCollections);
                    keys.Add(Menus.OpenConsole);
                    keys.Add(Menus.SystemStatus);
                }
            }

            if (nodeData is DatabaseNode)
            {
                keys.Add(Menus.DeleteDatabase);
                keys.Add(Menus.EvaluateJavaScript);
                keys.Add(Menus.CopyCollections);
                keys.Add(Menus.OpenConsole);
                keys.Add(Menus.SystemStatus);
                keys.Add(Menus.DeleteColsDBs);
            }

            if (nodeData is TreeViewRootNodeData)
            {
                var rootNode = nodeData as TreeViewRootNodeData;
                if (rootNode != null)
                {
                    if (rootNode.ConnectionInfo.ServiceId == MongoPlugin.PluginId)
                    {
                        keys.Add(Menus.NewDatabase);
                        keys.Add(Menus.WatchLogs);
                        keys.Add(Menus.CopyCollections);
                        keys.Add(Menus.OpenConsole);
                        keys.Add(Menus.SystemStatus);
                        keys.Add(Menus.DeleteColsDBs);
                    }
                }
            }
            if (nodeData is ReplicaSetMemberNode)
            {
                keys.Add(Menus.NewDatabase);
                keys.Add(Menus.CompactCollections);
                keys.Add(Menus.WatchLogs);
                keys.Add(Menus.CopyCollections);
                keys.Add(Menus.OpenConsole);
                keys.Add(Menus.SystemStatus);
                keys.Add(Menus.DeleteColsDBs);
            }
            else if (nodeData is DatabasesEmptyNode)
            {
                keys.Add(Menus.NewDatabase);
                keys.Add(Menus.CompactCollections);
            }

            if (nodeData is CollectionNode)
            {
                keys.Add(Menus.ViewCollection);
                keys.Add(Menus.DeleteCollection);
                keys.Add(Menus.RenameCollection);
                keys.Add(Menus.CopyCollection);
                keys.Add(Menus.CopyCollections);
                keys.Add(Menus.ManageIndexes);
                keys.Add(Menus.OpenConsole);
                keys.Add(Menus.SystemStatus);
            }

            if (nodeData is CollectionsNode)
            {
                keys.Add(Menus.CopyCollections);
                keys.Add(Menus.OpenConsole);
                keys.Add(Menus.SystemStatus);
                keys.Add(Menus.DeleteColsDBs);
            }

            if (nodeData is CollectionIndexesNode)
            {
                keys.Add(Menus.ManageIndexes);
            }
            if (nodeData is CollectionIndexNode)
            {
                keys.Add(Menus.ManageIndexes);
            }

            return keys;
        }
    }
}
