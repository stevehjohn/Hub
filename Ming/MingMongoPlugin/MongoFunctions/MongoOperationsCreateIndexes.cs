using MingMongoPlugin.DataObjects;
using MingPluginInterfaces;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.MongoFunctions
{
    internal partial class MongoOperations
    {
        public void CreateIndexes(ConnectionInfo cnn, string database, string collection, IEnumerable<IndexDescriptor> indexes)
        {
            var operation = new OperationStatus
            {
                IsIndeterminate = true,
                Title = Properties.Resources.ManageIndexes_Applying,
                Description = Properties.Resources.ManageIndexes_Dropping
            };
            _mingApp.AddLongRunningOperation(operation);

            var task = new CancelableTask(() => DoCreateIndexes(operation, cnn, database, collection, indexes), null);
            task.Execute();
        }

        public void DoCreateIndexes(OperationStatus operation, ConnectionInfo cnn, string database, string collection, IEnumerable<IndexDescriptor> indexes)
        {
            var col = MongoUtilities.Create(cnn).GetDatabase(database).GetCollection(collection);

            try
            {
                col.DropAllIndexes();
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }

            var count = 0;
            var errors = new List<string>();
            foreach (var index in indexes)
            {
                operation.PercentComplete = (int) ((100.0 / (double)indexes.Count()) * (double) count);
                operation.Description = string.Format(Properties.Resources.ManageIndexes_Creating, ++count);

                try
                {
                    var keys = new IndexKeysBuilder();

                    foreach (var property in index.IndexedProperties)
                    {
                        switch (property.IndexType)
                        {
                            case IndexType.Descending:
                                keys.Descending(property.PropertyName);
                                break;
                            case IndexType.Geospatial:
                                keys.GeoSpatial(property.PropertyName);
                                break;
                            default:
                                keys.Ascending(property.PropertyName);
                                break;
                        }
                    }

                    var options = new IndexOptionsBuilder();
                    options.SetSparse(index.IsSparse);
                    options.SetUnique(index.IsUnique);

                    col.CreateIndex(keys, options);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("2d has to be first"))
                    {
                        errors.Add(string.Format(Properties.Resources.ManageIndexes_GeospatialNotFirst, count));
                    }
                    else if (ex.Message.Contains("geo field") || ex.Message.Contains("location object expected"))
                    {
                        errors.Add(string.Format(Properties.Resources.ManageIndexes_InvalidGeospatial, count));
                    }
                    else if (ex.Message.Contains("duplicate key"))
                    {
                        errors.Add(string.Format(Properties.Resources.ManageIndexes_InvalidUnique, count));
                    }
                    else
                    {
                        errors.Add(string.Format(Properties.Resources.ManageIndexes_UnknownError, count, ex.Message));
                        Utilities.LogException(ex);
                    }
                }
            }

            operation.IsComplete = true;
            if (errors.Count == 0)
            {
                operation.IsSuccess = true;
                operation.Description = string.Format(Properties.Resources.ManageIndexes_Success, indexes.Count());
            }
            else
            {
                operation.IsSuccess = false;
                var errorString = new StringBuilder();
                errors.ToList().ForEach(
                    error =>
                    {
                        errorString.Append("\n");
                        errorString.Append(error);
                    });
                operation.Description = string.Format(Properties.Resources.ManageIndexes_Fail, errors.Count, errorString);
            }
        }
    }
}
