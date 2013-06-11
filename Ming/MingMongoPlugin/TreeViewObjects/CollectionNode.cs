using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.TreeViewObjects
{
    internal class CollectionNode : IMongoPluginTreeViewObject
    {
        private readonly string _databaseName;
        private readonly string _collectionName;

        public CollectionNode(string databaseName, string collectionName)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
        }

        public string DatabaseName { get { return _databaseName; } }
        public string CollectionName { get { return _collectionName; } }
    }
}
