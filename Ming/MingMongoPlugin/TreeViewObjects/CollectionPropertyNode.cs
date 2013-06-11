using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.TreeViewObjects
{
    internal class CollectionPropertyNode : IMongoPluginTreeViewObject
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly string _propertyName;
        private readonly string _fullPath;

        public CollectionPropertyNode(string databaseName, string collectionName, string propertyName, string fullPath)
        {
            _databaseName = databaseName;
            _collectionName = collectionName;
            _propertyName = propertyName;
            _fullPath = fullPath;
        }

        public string DatabaseName { get { return _databaseName; } }
        public string CollectionName { get { return _collectionName; } }
        public string PropertyName { get { return _propertyName; } }
        public string FullPath { get { return _fullPath; } }
    }
}
