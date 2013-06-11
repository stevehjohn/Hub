using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.TreeViewObjects
{
    internal class CollectionIndexNode : IMongoPluginTreeViewObject
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly string _indexName;

        public CollectionIndexNode(string databaseName, string collectionName, string indexName)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _indexName = indexName;
        }

        public string DatabaseName { get { return _databaseName; } }
        public string CollectionName { get { return _collectionName; } }
        public string IndexName { get { return _indexName; } }
    }
}
