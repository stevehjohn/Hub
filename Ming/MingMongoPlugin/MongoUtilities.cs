using MingPluginInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin
{
    internal static class MongoUtilities
    {
        public static MongoServer Create(ConnectionInfo cnnInfo)
        {
            if (!string.IsNullOrWhiteSpace(cnnInfo.Username))
            {
                return MongoServer.Create(string.Format("mongodb://{0}:{1}@{2}:{3}/?safe=true;slaveOk=true", 
                    cnnInfo.Username,
                    Security.SecureStringToString(cnnInfo.Password),
                    cnnInfo.Host, 
                    cnnInfo.Port));

                /*
                var client = new MongoClient(string.Format("mongodb://{0}:{1}@{2}:{3}/?safe=true;slaveOk=true",
                    cnnInfo.Username,
                    Security.SecureStringToString(cnnInfo.Password),
                    cnnInfo.Host,
                    cnnInfo.Port));

                return client.GetServer();*/

            }
            return MongoServer.Create(string.Format("mongodb://{0}:{1}/?safe=true;slaveOk=true", cnnInfo.Host, cnnInfo.Port));
        }

        public static IEnumerable<MongoReplicaSetMemberInfo> GetReplicaSetInfo(ConnectionInfo initialNode)
        {
            try
            {
                var cnn = Create(initialNode);
                BsonDocument rsState = null;

                try
                {
                    rsState = cnn.GetDatabase("local").Eval(EvalFlags.NoLock, "db._adminCommand({ replSetGetStatus: 1 })").AsBsonDocument;
                }
                catch { }
                if (rsState == null)
                {
                    return null;
                }
                var members = rsState["members"].AsBsonArray.ToList();
                var memberObjects = new List<MongoReplicaSetMemberInfo>();
                members.ForEach(member =>
                    {
                        var bson = member.AsBsonDocument;
                        var obj = new MongoReplicaSetMemberInfo();
                        var addr = bson["name"].AsString;
                        obj.Host = addr.Substring(0, addr.IndexOf(':'));
                        obj.Port = int.Parse(addr.Substring(addr.IndexOf(':') + 1));
                        obj.IsPrimary = bson["state"].AsInt32 == 1;
                        obj.IsArbiter = bson["state"].AsInt32 == 7;
                        memberObjects.Add(obj);
                    });
                return memberObjects.OrderByDescending(member => member.IsPrimary).ThenBy(member => member.Host);
            }
            catch
            {
                return null;
            }
        }

        public static bool TryConvertStringToBsonType(BsonType type, string value, out BsonValue bsonValue)
        {
            switch (type)
            {
                case BsonType.Binary:
                    try
                    {
                        bsonValue = BsonBinaryData.Create(Convert.FromBase64String(value));
                        return true;
                    }
                    catch
                    {
                        bsonValue = null;
                        return false;
                    }
                case BsonType.Boolean:
                    {
                        bool parsed;
                        if (bool.TryParse(value, out parsed))
                        {
                            bsonValue = BsonBoolean.Create(parsed);
                            return true;
                        }
                        bsonValue = null;
                        return false;
                    }
                case BsonType.DateTime:
                    {
                        DateTime parsed;
                        if (DateTime.TryParse(value, out parsed))
                        {
                            bsonValue = BsonDateTime.Create(parsed);
                            return true;
                        }
                        bsonValue = null;
                        return false;
                    }
                case BsonType.Double:
                    {
                        double parsed;
                        if (double.TryParse(value, out parsed))
                        {
                            bsonValue = BsonDouble.Create(parsed);
                            return true;
                        }
                        bsonValue = null;
                        return false;
                    }
                case BsonType.Int32:
                    {
                        Int32 parsed;
                        if (Int32.TryParse(value, out parsed))
                        {
                            bsonValue = BsonInt32.Create(parsed);
                            return true;
                        }
                        bsonValue = null;
                        return false;
                    }
                case BsonType.Int64:
                    {
                        Int64 parsed;
                        if (Int64.TryParse(value, out parsed))
                        {
                            bsonValue = BsonInt64.Create(parsed);
                            return true;
                        }
                        bsonValue = null;
                        return false;
                    }
                case BsonType.Null:
                    {
                        bsonValue = BsonNull.Value;
                        return true;
                    }
                case BsonType.ObjectId:
                    try
                    {
                        bsonValue = BsonObjectId.Create(value);
                        return true;
                    }
                    catch
                    {
                        bsonValue = null;
                        return false;
                    }
                case BsonType.String:
                    bsonValue = BsonString.Create(value);
                    return true;
                // TimeStamp?
            }

            throw new ArgumentOutOfRangeException(string.Format("Conversion to BsonType.{0} not supported", type.ToString()), "type");
        }

        public static string PrettyPrintJson(string json)
        {
            var sb = new StringBuilder();

            int indent = 0;
            bool inQuotes = false;
            char prev = '\0';
            bool prevNewLine = false;
            foreach (var c in json)
            {
                if (inQuotes)
                {
                    sb.Append(c);
                    if (c == '"' && prev != '\\')
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    switch (c)
                    {
                        case '"':
                            inQuotes = true;
                            sb.Append(c);
                            prevNewLine = false;
                            break;
                        case '{':
                        case '[':
                            if (!prevNewLine)
                            {
                                sb.Append('\n');
                                sb.Append(new string(' ', indent * 2));
                            }
                            sb.Append(c);
                            indent++;
                            sb.Append('\n');
                            sb.Append(new string(' ', indent * 2));
                            prevNewLine = true;
                            break;
                        case '}':
                        case ']':
                            indent--;
                            sb.Append('\n');
                            sb.Append(new string(' ', indent * 2));
                            sb.Append(c);
                            prevNewLine = false;
                            break;
                        case ',':
                            sb.Append(",\n");
                            sb.Append(new string(' ', indent * 2));
                            prevNewLine = true;
                            break;
                        case ':':
                            sb.Append(": ");
                            prevNewLine = false;
                            break;
                        case '\n':
                        case ' ':
                        case '\t':
                            break;
                        default:
                            sb.Append(c);
                            prevNewLine = false;
                            break;
                    }
                }
                prev = c;
            }

            return sb.ToString().Trim();
        }
    }
}
