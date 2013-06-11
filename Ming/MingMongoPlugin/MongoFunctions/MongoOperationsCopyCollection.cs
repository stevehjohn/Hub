using MingControls.Controllers;
using MingPluginInterfaces;
using System;

namespace MingMongoPlugin.MongoFunctions
{
    internal partial class MongoOperations
    {
        public void CopyCollection(MingTreeViewItem node, ConnectionInfo cnnInfo, string database, string collection)
        {
            var ted = new TextEntryDialogController(_mingApp.MainWindow, Properties.Resources.CopyCollection_Title, Properties.Resources.CreateCollection_Prompt);
            ted.Text = collection;
            if (!ted.ShowDialog())
            {
                return;
            }
            var name = ted.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            var operation = new OperationStatus
            {
                IsIndeterminate = true,
                Title = Properties.Resources.CopyCollection_OpTitle,
                Description = Properties.Resources.CopyCollection_Counting
            };
            _mingApp.AddLongRunningOperation(operation);
            var task = new CancelableTask(() => DoCopyCollection(node, operation, cnnInfo, database, collection, name), null);
            task.Execute();
        }

        private void DoCopyCollection(MingTreeViewItem node, OperationStatus operation, ConnectionInfo cnnInfo, string database, string collection, string newCollection)
        {
            try
            {
                if (MongoUtilities.Create(cnnInfo).GetDatabase(database).CollectionExists(newCollection))
                {
                    operation.IsSuccess = false;
                    operation.IsComplete = true;
                    operation.Description = string.Format(Properties.Resources.CopyCollection_Exists, newCollection);
                    return;
                }
                var src = MongoUtilities.Create(cnnInfo).GetDatabase(database).GetCollection(collection);
                var dest = MongoUtilities.Create(cnnInfo).GetDatabase(database).GetCollection(newCollection);
                var count = src.Count();
                operation.IsIndeterminate = false;
                operation.Description = string.Format(Properties.Resources.CopyCollection_Copying, 0);

                var copied = 0;
                var srcCur = src.FindAll().SetSnapshot();
                foreach (var item in srcCur)
                {
                    dest.Insert(item);
                    copied++;
                    operation.Description = string.Format(Properties.Resources.CopyCollection_Copying, copied);
                    operation.PercentComplete = (int)((100.0 / (double)count) * (double)copied);
                }
                operation.IsSuccess = true;
                operation.IsComplete = true;
            }
            catch (Exception ex)
            {
                operation.Description = string.Format(Properties.Resources.CopyCollections_Error, ex.Message);
                operation.IsSuccess = false;
                operation.IsComplete = true;
            }
            _mingApp.RefreshTreeViewItem(node.Parent);
        }
    }
}
