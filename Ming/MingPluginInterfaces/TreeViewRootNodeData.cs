namespace MingPluginInterfaces
{
    public class TreeViewRootNodeData : IConnectionProvider
    {
        private readonly ConnectionInfo _connectionInfo;

        public TreeViewRootNodeData(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public ConnectionInfo ConnectionInfo { get { return _connectionInfo; } }
    }
}
