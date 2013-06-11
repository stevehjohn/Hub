using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.TreeViewObjects
{
    internal class DatabaseNode : IMongoPluginTreeViewObject
    {
        private readonly string _databaseName;

        public DatabaseNode(string databaseName)
        {
            _databaseName = databaseName;
        }

        public string DatabaseName { get { return _databaseName; } }
    }
}
