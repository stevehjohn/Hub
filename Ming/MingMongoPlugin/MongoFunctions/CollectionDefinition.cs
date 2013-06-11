using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.MongoFunctions
{
    internal class CollectionDefinition
    {
        private readonly ConnectionInfo _connection;
        private readonly string _database;
        private readonly string _collection;

        public CollectionDefinition(ConnectionInfo connection, string database, string collection)
        {
            _connection = connection;
            _database = database;
            _collection = collection;
        }

        public ConnectionInfo Connection { get { return _connection; } }

        public string Database { get { return _database; } }

        public string Collection { get { return _collection; } }
    }
}
