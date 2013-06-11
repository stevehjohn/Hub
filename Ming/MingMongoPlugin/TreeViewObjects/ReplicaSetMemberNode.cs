using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.TreeViewObjects
{
    internal class ReplicaSetMemberNode : IMongoPluginTreeViewObject, IConnectionProvider
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _isPrimary;

        public ReplicaSetMemberNode(string host, int port, bool isPrimary)
        {
            _host = host;
            _port = port;
            _isPrimary = isPrimary;
        }

        public string Host { get { return _host; } }
        public int Port { get { return _port; } }
        public bool IsPrimary { get { return _isPrimary; } }

        public ConnectionInfo ConnectionInfo
        {
            get { return new ConnectionInfo(null, MongoPlugin.PluginId, _host, _port, null, null); }
        }
    }
}
