using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.MongoFunctions
{
    internal class CopyCollectionDefinition
    {
        private readonly CollectionDefinition _source;
        private readonly CollectionDefinition _target;

        public CopyCollectionDefinition(CollectionDefinition source, CollectionDefinition target)
        {
            _source = source;
            _target = target;
        }

        public CollectionDefinition Source { get { return _source; } }

        public CollectionDefinition Target { get { return _target; } }
    }
}
