using MingPluginInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.Mongo
{
    internal class CollectionProperty
    {
        private readonly string _name;
        private readonly string _fullName;
        private readonly BsonType _type;

        public Dictionary<string, CollectionProperty> Children { get; set; }

        public string Name { get { return _name; } }
        public string FullName { get { return _fullName; } }
        public BsonType Type { get { return _type; } }

        public CollectionProperty(string name, BsonType type, string fullName)
        {
            _name = name;
            _type = type;
            _fullName = fullName;
            Children = new Dictionary<string, CollectionProperty>();
        }
    }

    internal class CollectionPropertyAnalyser
    {
        private const int DocumentsToAnalyse = 1000;

        private readonly MongoServer _server;
        private readonly string _database;
        private readonly string _collection;

        private Dictionary<string, CollectionProperty> _properties;

        public CollectionPropertyAnalyser(MongoServer server, string database, string collection)
        {
            _server = server;
            _database = database;
            _collection = collection;

            AnalyseCollection();
        }

        private void AnalyseCollection()
        {
            var collection = _server.GetDatabase(_database).GetCollection(_collection);

            var data = collection.Find(null).SetLimit(DocumentsToAnalyse);

            _properties = new Dictionary<string, CollectionProperty>();

            data.ToList().ForEach(doc => AnalyseDocument(doc));
        }

        private void AnalyseDocument(BsonDocument document)
        {
            ScanProperties(document.ToArray(), _properties, "");
        }

        private void ScanProperties(BsonElement[] properties, Dictionary<string, CollectionProperty> parent, string path)
        {
            foreach (var property in properties)
            {
                var fullName = string.IsNullOrWhiteSpace(path) ? property.Name : string.Format("{0}.{1}", path, property.Name);
                CollectionProperty toScan;
                if (!parent.ContainsKey(property.Name))
                {
                    toScan = new CollectionProperty(property.Name, property.Value.BsonType, fullName);
                    parent.Add(property.Name, toScan);
                }
                else 
                {
                    toScan = parent[property.Name];
                }
                if (toScan.Type == BsonType.Document)
                {
                    if (property.Value != BsonNull.Value)
                    {
                        ScanProperties(property.Value.AsBsonDocument.ToArray(), toScan.Children, fullName);
                    }
                }
                if (toScan.Type == BsonType.Array)
                {
                    property.Value.AsBsonArray.Take(DocumentsToAnalyse).ToList().ForEach(item =>
                        {
                            if (item.BsonType != BsonType.Array)
                            {
                                if (item.BsonType == BsonType.Document)
                                {
                                    ScanProperties(item.AsBsonDocument.ToArray(), toScan.Children, fullName);
                                }
                                else
                                {
                                    if (! toScan.Children.ContainsKey("[]"))
                                    {
                                        toScan.Children.Add("[]", new CollectionProperty("[]", item.BsonType, fullName));
                                    }
                                }
                            }
                        });
                }
            }
        }

        public IEnumerable<CollectionProperty> GetProperties(string path)
        {
            var properties = _properties;
            if (!string.IsNullOrEmpty(path))
            {
                var nodes = path.Split('.').ToList();

                while (nodes.Count > 0)
                {
                    properties = properties[nodes[0]].Children;
                    nodes.RemoveAt(0);
                }
            }

            return properties.Select(item => item.Value).OrderBy(property => property.Name);
        }

        public IEnumerable<CollectionProperty> GetAllProperties()
        {
            var flat = new List<CollectionProperty>();

            AddPropertiesToList(flat, _properties.Select(item => item.Value));

            return flat.OrderBy(property => property.Name);
        }

        private void AddPropertiesToList(IList<CollectionProperty> list, IEnumerable<CollectionProperty> properties)
        {
            properties.ToList().ForEach(property =>
                {
                    list.Add(property);
                    AddPropertiesToList(list, property.Children.Select(item => item.Value));
                });
        }
    }
}
