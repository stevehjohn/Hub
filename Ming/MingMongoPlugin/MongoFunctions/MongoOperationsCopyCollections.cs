using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.MongoFunctions
{
    internal partial class MongoOperations
    {
        public void CopyCollections(IEnumerable<CopyCollectionDefinition> collections)
        {
            var operation = new OperationStatus
            {
                IsIndeterminate = true,
                Title = string.Format(Properties.Resources.CopyingCollections_Title, collections.Count()),
                Description = Properties.Resources.CopyingCollections_Counting
            };
            _mingApp.AddLongRunningOperation(operation);

            var task = new CancelableTask(() => DoCopyCollections(operation, collections), null);
            task.Execute();
        }

        private void DoCopyCollections(OperationStatus operation, IEnumerable<CopyCollectionDefinition> collections)
        {
            CopyCollectionDefinition currentJob = null;
            try
            {
                long docs = 0;
                var colsList = collections.ToList();
                colsList.ForEach(item =>
                    docs += MongoUtilities.Create(item.Source.Connection).GetDatabase(item.Source.Database).GetCollection(item.Source.Collection).Count());

                operation.PercentComplete = 0;
                operation.IsIndeterminate = false;
                long copied = 0;
                foreach (var job in collections)
                {
                    currentJob = job;

                    var src = MongoUtilities.Create(job.Source.Connection).GetDatabase(job.Source.Database).GetCollection(job.Source.Collection);
                    var dest = MongoUtilities.Create(job.Target.Connection).GetDatabase(job.Target.Database).GetCollection(job.Target.Collection);

                    // TODO: Batch these up
                    var srcCur = src.FindAll().SetSnapshot();
                    var thisCol = 0;
                    foreach (var item in srcCur)
                    {
                        dest.Insert(item);
                        copied++;
                        thisCol++;

                        operation.Description = string.Format(Properties.Resources.CopyingCollections_Copying,
                            job.Source.Database, job.Source.Collection,
                            job.Target.Database, job.Target.Collection,
                            thisCol);
                        var percent = (int)((100.0 / (double)docs) * (double)copied);
                        if (percent > 100)
                        {
                            operation.IsIndeterminate = true;
                        }
                        else
                        {
                            operation.PercentComplete = percent;
                        }
                    }

                    operation.Description = string.Format(Properties.Resources.CopyingCollections_CreatingIndexes,
                        job.Target.Database, job.Target.Collection);
                    foreach (var idx in MongoUtilities.Create(job.Source.Connection).GetDatabase(job.Source.Database).GetCollection(job.Source.Collection).GetIndexes())
                    {
                        MongoUtilities.Create(job.Target.Connection).GetDatabase(job.Target.Database).GetCollection(job.Target.Collection).CreateIndex(idx.Key);
                    }
                }

                operation.IsSuccess = true;
                operation.IsComplete = true;
                operation.Description = string.Format(Properties.Resources.CopyingCollections_Complete, collections.Count(), copied);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not master"))
                {
                    operation.Description = string.Format(Properties.Resources.CopyingCollections_NotMaster, currentJob.Target.Connection.Host, currentJob.Target.Connection.Port);
                }
                else if (ex.Message.Contains("invalid collection name"))
                {
                    operation.Description = string.Format(Properties.Resources.CopyingCollections_InvalidName, currentJob.Target.Collection);
                }
                else
                {
                    operation.Description = string.Format(Properties.Resources.CopyCollections_Error, ex.Message);
                }
                operation.IsSuccess = false;
                operation.IsComplete = true;
            }
        }
    }
}
