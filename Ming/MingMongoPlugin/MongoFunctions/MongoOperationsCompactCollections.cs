using MingPluginInterfaces;
using MingPluginInterfaces.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MingMongoPlugin.MongoFunctions
{
    internal partial class MongoOperations
    {
        public void CompactCollections(MingTreeViewItem node, ConnectionInfo cnnInfo, string database)
        {
            var confirm = new MessageBox();
            var message = database == null
                ? string.Format(Properties.Resources.CompactCollections_ConfirmAll, cnnInfo.Host, cnnInfo.Port)
                : string.Format(Properties.Resources.CompactCollections_Confirm, database);
            if (!confirm.ShowConfirm(_mingApp.MainWindow, message))
            {
                return;
            }

            var operation = new OperationStatus
            {
                IsIndeterminate = true,
                Title = Properties.Resources.CompactCollections_Title,
                Description = Properties.Resources.CompactCollections_DescCounting
            };
            _mingApp.AddLongRunningOperation(operation);
            var task = new CancelableTask(() => DoCompactCollections(operation, cnnInfo, database), null);
            task.Execute();
        }

        private void DoCompactCollections(OperationStatus operation, ConnectionInfo cnnInfo, string database)
        {
            try
            {
                var databases = new List<string>();
                var cnn = MongoUtilities.Create(cnnInfo);
                if (database != null)
                {
                    databases.Add(database);
                }
                else
                {
                    cnn.GetDatabaseNames().ToList().ForEach(item => databases.Add(item));
                }
                var collections = new List<Tuple<string, string>>();
                databases.ForEach(db => cnn.GetDatabase(db).GetCollectionNames().ToList().ForEach(col => collections.Add(new Tuple<string, string>(db, col))));
                var done = 0;
                operation.IsIndeterminate = false;
                foreach (var col in collections)
                {
                    operation.Description = string.Format(Properties.Resources.CompactCollections_DescCompacting, col.Item1, col.Item2);
                    operation.PercentComplete = (int)((100.0 / (double)collections.Count) * (double)done);
                    cnn.GetDatabase(col.Item1).Eval(string.Format("db.runCommand({{ compact: \"{0}\", force: true }})", col.Item2));
                    done++;
                }
                operation.Description = Properties.Resources.CompactCollections_DescComplete;
                operation.IsSuccess = true;
                operation.IsComplete = true;
            }
            catch (Exception ex)
            {
                operation.Description = string.Format(Properties.Resources.CompactCollections_Error, ex.Message);
                operation.IsSuccess = false;
                operation.IsComplete = true;
            }
        }
    }
}
