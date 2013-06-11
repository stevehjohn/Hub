using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin
{
    internal class MongoReplicaSetMemberInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsArbiter { get; set; }
    }
}
