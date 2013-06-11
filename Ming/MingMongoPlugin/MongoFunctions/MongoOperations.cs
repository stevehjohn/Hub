using MingPluginInterfaces;

namespace MingMongoPlugin.MongoFunctions
{
    internal partial class MongoOperations
    {
        private IMingApp _mingApp;

        public IMingApp MingApp
        {
            set
            {
                _mingApp = value;
            }
        }
    }
}
