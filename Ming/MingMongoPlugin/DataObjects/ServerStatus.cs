using MingPluginInterfaces;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.DataObjects
{
    internal enum LockType
    {
        Read,
        Write
    }

    internal class ServerStatus
    {
        private readonly BsonDocument _status;

        private long _networkBytesIn;
        private long _networkBytesOut;
        private long _virtualMemoryMB;
        private long _connections;
        private long _totalOps;

        public long NetworkBytesIn { get { return _networkBytesIn; } }
        public long NetworkBytesOut { get { return _networkBytesOut; } }
        public long VirtualMemoryMB { get { return _virtualMemoryMB; } }
        public long Connections { get { return _connections; } }
        public long TotalOps { get { return _totalOps; } }

        public long GetLockTimeMicros(string database, LockType lockType)
        {
            try
            {
                var valToGet = string.Empty;
                if (lockType == LockType.Read)
                {
                    valToGet = ".r";
                }
                else
                {
                    valToGet = ".w";
                }
                if (string.IsNullOrWhiteSpace(database))
                {
                    valToGet = valToGet.ToUpper();
                    valToGet = "locks.$.timeLockedMicros" + valToGet;
                }
                else
                {
                    valToGet = string.Format("locks.{0}.timeLockedMicros", database) + valToGet;
                }
                return GetValue<long>(valToGet);
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
            return 0;
        }

        public static string MonitoredLockDescription(string database, LockType lockType)
        {
            string desc;
            if (!string.IsNullOrWhiteSpace(database))
            {
                desc = database;
            }
            else
            {
                desc = Properties.Resources.ServerStatus_Global;
            }
            string type;
            if (lockType == LockType.Read)
            {
                type = Properties.Resources.ServerStatus_Read;
            }
            else
            {
                type = Properties.Resources.ServerStatus_Write;
            }
            return string.Format(Properties.Resources.ServerStatus_LockDesc, desc, type);
        }

        private ServerStatus() { }

        public ServerStatus(BsonDocument status)
        {
            _status = status;
            ParseStatusDocument();
        }

        private T GetValue<T>(string property)
        {
            var path = property.Split('.');
            var level = 0;
            var obj = _status;
            while (level < path.Length)
            {
                if (path[level] == "$")
                {
                    path[level] = ".";
                }
                if (obj.Contains(path[level]))
                {
                    if (level == path.Length - 1)
                    {
                        T ret;
                        try
                        {
                            ret = (T)obj[path[level]].RawValue;
                        }
                        catch
                        {
                            return default(T);
                        }
                        return ret;
                    }
                    else
                    {
                        obj = obj[path[level]].AsBsonDocument;
                        level++;
                    }
                }
                else
                {
                    break;
                }
            }
            return default(T);
        }

        private void ParseStatusDocument()
        {
            try
            {
                _networkBytesIn = GetValue<int>("network.bytesIn");
                _networkBytesOut = GetValue<int>("network.bytesOut");
                _virtualMemoryMB = GetValue<int>("mem.virtual");
                _connections = GetValue<int>("connections.current");

                _totalOps = GetValue<int>("opcounters.insert");
                _totalOps += GetValue<int>("opcounters.query");
                _totalOps += GetValue<int>("opcounters.update");
                _totalOps += GetValue<int>("opcounters.delete");
                _totalOps += GetValue<int>("opcounters.getmore");
                _totalOps += GetValue<int>("opcounters.command");
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        public static ServerStatus operator -(ServerStatus a, ServerStatus b)
        {
            return new ServerStatus
            {
                _networkBytesIn = a._networkBytesIn - b.NetworkBytesIn,
                _networkBytesOut = a._networkBytesOut - b.NetworkBytesOut,
                _totalOps = a._totalOps - b.TotalOps
            };
        }
    }
}
