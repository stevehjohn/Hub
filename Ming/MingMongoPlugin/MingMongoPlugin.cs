using MingMongoPlugin.Properties;
using MingPluginInterfaces;
using MongoDB.Driver;
using System.Collections.Generic;
using MingMongoPlugin.TreeViewObjects;
using MingMongoPlugin.TabDocuments;
using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using MingPluginInterfaces.Forms;
using MingControls.Controllers;
using MingMongoPlugin.MongoFunctions;
using MingMongoPlugin.Dialogs;
using System.IO;
using Microsoft.Win32;

namespace MingMongoPlugin
{
    public class MongoPlugin : IMingPlugin, IMingMenuClient
    {
        private readonly IMingTreeViewClient _treeViewClient;
        private IMingApp _mingApp;
        private readonly MongoOperations _mongoOps; 

        internal const string PluginId = "mongo";

        public MongoPlugin()
        {
            _treeViewClient = new MingTreeViewClient();
            _mongoOps = new MongoOperations();
        }

        public IMingApp MingApp
        {
            set 
            { 
                _mingApp = value;
                _mongoOps.MingApp = _mingApp;
            }
        }

        public string Id
        {
            get { return PluginId; }
        }

        public string Name
        {
            get { return Resources.PluginTitle; }
        }

        public string Description
        {
            get { return Resources.PluginDescription; }
        }

        public string Version
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            }
        }
        
        public int DefaultPort
        {
            get { return 27017; }
        }

        public System.Drawing.Bitmap TreeViewIcon
        {
            get { return Properties.Resources.mongo_treeview; }
        }

        public bool Test(ConnectionInfo cnn)
        {
            try
            {
                MongoUtilities.Create(cnn).Ping();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public IMingTreeViewClient TreeViewClient
        {
            get { return _treeViewClient; }
        }

        public IEnumerable<MingMenuItem> AllMenuItems
        {
            get
            {
                return Menus.GetMenus();
            }
        }

        private TType FindParentWithDataOfType<TType>(MingTreeViewItem node) where TType : class
        {
            if (node != null && node.Data is TType)
            {
                return node.Data as TType;
            }

            while (node != null)
            {
                node = node.Parent;
                if (node != null)
                {
                    if (node.Data is TType)
                    {
                        return node.Data as TType;
                    }
                }
            }
            return null;
        }

        public void MenuItemHandler(ConnectionInfo cnnInfo, MingTreeViewItem node, string menuKey)
        {
            switch (menuKey)
            {
                case Menus.DeleteColsDBs:
                    DeleteCollectionsAndDatabases(cnnInfo, node);
                    break;
                case Menus.SystemStatus:
                    SystemStatus(cnnInfo);
                    break;
                case Menus.ManageIndexes:
                    ManageIndexes(cnnInfo, node);
                    break;
                case Menus.OpenConsole:
                    OpenConsole(cnnInfo, node);
                    break;
                case Menus.CopyCollections:
                    CopyCollections(cnnInfo, node);
                    break;
                case Menus.WatchLogs:
                    var logView = new LogWatch(cnnInfo);
                    _mingApp.AddDocumentTab(logView, PluginId);
                    break;
                case Menus.DeleteDocument:
                    break;
                case Menus.CopyCollection:
                    _mongoOps.CopyCollection(node, cnnInfo, FindParentWithDataOfType<DatabaseNode>(node).DatabaseName, (node.Data as CollectionNode).CollectionName);
                    break;
                case Menus.CompactCollections:
                    {
                        var dbNode = FindParentWithDataOfType<DatabaseNode>(node);
                        if (dbNode == null)
                        {
                            _mongoOps.CompactCollections(node, cnnInfo, null);
                        }
                        else
                        {
                            _mongoOps.CompactCollections(node, cnnInfo, dbNode.DatabaseName);
                        }
                    }
                    break;
                case Menus.EvaluateJavaScript:
                    {
                        var data = node.Data as DatabaseNode;
                        var evalView = new EvaluateJavaScript(cnnInfo, data.DatabaseName);
                        evalView.BusyStateChanged += BusyStateChanged;
                        _mingApp.AddDocumentTab(evalView, PluginId);
                    }
                    break;
                case Menus.NewDatabase:
                    CreateDatabase(node, cnnInfo);
                    break;
                case Menus.NewCollection:
                    CreateCollection(node, cnnInfo, FindParentWithDataOfType<DatabaseNode>(node).DatabaseName);
                    break;
                case Menus.RenameCollection:
                    {
                        var data = node.Data as CollectionNode;
                        RenameCollection(node, cnnInfo, FindParentWithDataOfType<DatabaseNode>(node).DatabaseName, data.CollectionName);
                    }
                    break;
                case Menus.ViewCollection:
                    {
                        var data = node.Data as CollectionNode;
                        var collectionView = new CollectionView(cnnInfo, FindParentWithDataOfType<DatabaseNode>(node).DatabaseName, data.CollectionName);
                        collectionView.BusyStateChanged += BusyStateChanged;
                        collectionView.MenuStateChanged += MenuStateChanged;
                        _mingApp.AddDocumentTab(collectionView, PluginId);
                        collectionView.ShowData();
                    }
                    break;
                case Menus.DeleteCollection:
                    DeleteCollection(cnnInfo, node);
                    break;
                case Menus.DeleteDatabase:
                    DeleteDatabase(cnnInfo, node);
                    break;
            }
        }

        private void DeleteCollectionsAndDatabases(ConnectionInfo cnnInfo, MingTreeViewItem node)
        {
            var dlg = new DeleteCollectionsAndDatabases(_mingApp.MainWindow, cnnInfo);
            dlg.ShowDialog();
        }

        private void SystemStatus(ConnectionInfo cnnInfo)
        {
            var ss = new SystemStatus(cnnInfo);
            _mingApp.AddDocumentTab(ss, PluginId);
        }

        private void OpenConsole(ConnectionInfo cnnInfo, MingTreeViewItem node)
        {
            var path = Properties.Settings.Default.MongoExeLocation;
            if (!string.IsNullOrEmpty(path))
            {
                path = path.ToLower();
                if (!path.EndsWith("mongo.exe"))
                {
                    path = null;
                }
                else if (!File.Exists(path))
                {
                    path = null;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                var msg = new MessageBox();
                msg.ShowMessage(_mingApp.MainWindow, Properties.Resources.Console_LocateDesc, Properties.Resources.Console_LocateTitle);

                var ofg = new OpenFileDialog();
                ofg.CheckPathExists = true;
                ofg.FileName = "mongo.exe";
                ofg.Filter = "Mongo Console|mongo.exe";

                if (ofg.ShowDialog() == true)
                {
                    path = ofg.FileName.ToLower();
                    if (string.IsNullOrEmpty(path) || !path.EndsWith("mongo.exe"))
                    {
                        msg = new MessageBox();
                        msg.ShowMessage(_mingApp.MainWindow, Properties.Resources.Console_WrongFileDesc, Properties.Resources.Console_WrongFileTitle);
                        return;
                    }
                    Properties.Settings.Default.MongoExeLocation = path;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    return;
                }
            }

            string db = null;
            var dbNode = FindParentWithDataOfType<DatabaseNode>(node);
            if (dbNode != null)
            {
                db = dbNode.DatabaseName;
            }

            var console = new MongoConsole(path, cnnInfo, db);
            _mingApp.AddDocumentTab(console, PluginId);
        }

        private void ManageIndexes(ConnectionInfo cnnInfo, MingTreeViewItem node)
        {
            var colNode = FindParentWithDataOfType<CollectionNode>(node);
            var dlg = new ManageIndexes(_mingApp.MainWindow, cnnInfo, colNode.DatabaseName, colNode.CollectionName);

            if (dlg.ShowDialog())
            {
                _mongoOps.CreateIndexes(cnnInfo, colNode.DatabaseName, colNode.CollectionName, dlg.Indexes);
            }
        }

        private void CopyCollections(ConnectionInfo cnnInfo, MingTreeViewItem node)
        {
            var copyCols = new CopyCollections(_mingApp.MainWindow, cnnInfo, _mingApp.GetConnectionsForPlugin(PluginId));
            if (node.Data is DatabaseNode)
            {
                copyCols.DatabaseName = ((DatabaseNode)node.Data).DatabaseName;
            }
            else if (node.Data is CollectionNode)
            {
                copyCols.DatabaseName = FindParentWithDataOfType<DatabaseNode>(node).DatabaseName;
                copyCols.CollectionName = ((CollectionNode)node.Data).CollectionName;
            }
            else if (node.Data is CollectionsNode)
            {
                copyCols.DatabaseName = FindParentWithDataOfType<DatabaseNode>(node).DatabaseName;
            }

            if (copyCols.ShowDialog())
            {
                if (copyCols.CollectionsToCopy != null && copyCols.CollectionsToCopy.Count() > 0)
                {
                    _mongoOps.CopyCollections(copyCols.CollectionsToCopy);
                }
            }
        }

        void MenuStateChanged(string menuKey, bool enabled, int tabDocumentId)
        {
            _mingApp.SetMenuItemStateForTab(menuKey, enabled, tabDocumentId);
        }


        public void CreateDatabase(MingTreeViewItem node, ConnectionInfo cnnInfo)
        {
            var ted = new TextEntryDialogController(_mingApp.MainWindow, Properties.Resources.CreateDatabase_Title, Properties.Resources.CreateDatabase_Prompt);
            if (ted.ShowDialog())
            {
                var name = ted.Text.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var operation = new OperationStatus
                    {
                        IsIndeterminate = true,
                        Title = string.Format(Properties.Resources.CreateDatabase_OperationTitle, name),
                        Description = string.Format(Properties.Resources.CreateDatabase_OperationTitle, name)
                    };
                    _mingApp.AddLongRunningOperation(operation);
                    var task = new CancelableTask<string>(
                        () => DoCreateDatabase(node, cnnInfo, name),
                        error => CreateDatabaseComplete(error, operation));
                    task.Execute();
                }
            }
        }

        public string DoCreateDatabase(MingTreeViewItem node, ConnectionInfo cnnInfo, string name)
        {
            if (MongoUtilities.Create(cnnInfo).DatabaseExists(name))
            {
                return string.Format(Properties.Resources.CreateDatabase_Exists, name);
            }
            try
            {
                var db = MongoUtilities.Create(cnnInfo).GetDatabase(name);
                db.CreateCollection("dummy");
                db.DropCollection("dummy");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("is not valid"))
                {
                    return string.Format(Properties.Resources.CreateDatabase_InvalidName, name);
                }
                if (ex.Message.Contains("not master"))
                {
                    return string.Format(Properties.Resources.Renaming_NotMaster, cnnInfo.Host, cnnInfo.Port);
                }
                return string.Format(Properties.Resources.Renaming_GenericError, ex.Message);
            }
            _mingApp.RefreshTreeViewItem(node);
            return "";
        }

        public void CreateDatabaseComplete(string error, OperationStatus operation)
        {
            if (!string.IsNullOrEmpty(error))
            {
                operation.Description = error;
            }
            operation.IsSuccess = string.IsNullOrWhiteSpace(error);
            operation.IsComplete = true;
        }

        public void CreateCollection(MingTreeViewItem node, ConnectionInfo cnnInfo, string database)
        {
            TextEntryDialogController ted = new TextEntryDialogController(_mingApp.MainWindow, Properties.Resources.CreateCollection_Title, Properties.Resources.CreateCollection_Prompt);
            if (ted.ShowDialog())
            {
                var name = ted.Text.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var operation = new OperationStatus
                    {
                        IsIndeterminate = true,
                        Title = string.Format(Properties.Resources.CreateCollection_OperationTitle, name),
                        Description = string.Format(Properties.Resources.CreateCollection_OperationTitle, name)
                    };
                    _mingApp.AddLongRunningOperation(operation);
                    var task = new CancelableTask<string>(
                        () => DoCreateCollection(node, cnnInfo, database, name),
                        error => CollectionCreateDone(error, operation));
                    task.Execute();
                }
            }
        }

        private string DoCreateCollection(MingTreeViewItem node, ConnectionInfo cnnInfo, string database, string name)
        {
            try
            {
                MongoUtilities.Create(cnnInfo).GetDatabase(database).CreateCollection(name);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("invalid ns"))
                {
                    return string.Format(Properties.Resources.Renaming_InvalidName, name);
                }
                if (ex.Message.Contains("collection already exists"))
                {
                    return string.Format(Properties.Resources.Renaming_CollectionExists, name);
                }
                if (ex.Message.Contains("not master"))
                {
                    return string.Format(Properties.Resources.Renaming_NotMaster, cnnInfo.Host, cnnInfo.Port);
                }
                return string.Format(Properties.Resources.Renaming_GenericError, ex.Message);
            }
            if (node.Data is DatabaseNode)
            {
                _mingApp.RefreshTreeViewItem(node.Items[0] as MingTreeViewItem);
            }
            else
            {
                _mingApp.RefreshTreeViewItem(node);
            }
            return "";
        }

        private void CollectionCreateDone(string error, OperationStatus operation)
        {
            if (!string.IsNullOrEmpty(error))
            {
                operation.Description = error;
            }
            operation.IsSuccess = string.IsNullOrWhiteSpace(error);
            operation.IsComplete = true;
        }

        public void RenameCollection(MingTreeViewItem node, ConnectionInfo cnnInfo, string database, string collection)
        {
            TextEntryDialogController ted = new TextEntryDialogController(_mingApp.MainWindow, Properties.Resources.RenameCollection_Title, Properties.Resources.RenameCollection_Prompt);
            ted.Text = collection;
            if (ted.ShowDialog())
            {
                var newName = ted.Text.Trim();
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    var operation = new OperationStatus 
                        { 
                            IsIndeterminate = true, 
                            Title = string.Format(Properties.Resources.Renaming_Title, collection),
                            Description = string.Format(Properties.Resources.Renaming_Description, collection, newName)
                        };
                    _mingApp.AddLongRunningOperation(operation);
                    var task = new CancelableTask<string>(
                        () => DoRenameCollection(node, cnnInfo, database, collection, newName, operation),
                        error => CollectionRenameDone(error, operation));
                    task.Execute();
                }
            }
        }

        private string DoRenameCollection(MingTreeViewItem node, ConnectionInfo cnnInfo, string database, string targetCollection, string newName, OperationStatus status)
        {
            try
            {
                var cnn = MongoUtilities.Create(cnnInfo).GetDatabase(database).RenameCollection(targetCollection, newName);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("invalid collection name"))
                {
                    return string.Format(Properties.Resources.Renaming_InvalidName, newName);
                }
                if (ex.Message.Contains("target namespace exists"))
                {
                    return string.Format(Properties.Resources.Renaming_CollectionExists, newName);
                }
                if (ex.Message.Contains("not master"))
                {
                    return string.Format(Properties.Resources.Renaming_NotMaster, cnnInfo.Host, cnnInfo.Port);
                }
                return string.Format(Properties.Resources.Renaming_GenericError, ex.Message);
            }
            _mingApp.RefreshTreeViewItem(node.Parent);
            return "";
        }

        private void CollectionRenameDone(string error, OperationStatus operation)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                operation.Description = error;
            }
            operation.IsSuccess = string.IsNullOrWhiteSpace(error);
            operation.IsComplete = true;
        }

        public void DeleteCollection(ConnectionInfo cnnInfo, MingTreeViewItem node)
        {
            if (node == null)
            {
                return;
            }

            var col = node.Data as CollectionNode;
            if (col == null)
            {
                return;
            }

            var confirm = new MessageBox();
            if (!confirm.ShowConfirm(_mingApp.MainWindow, string.Format(Properties.Resources.Confirm_DeleteCollection, col.CollectionName)))
            {
                return;
            }

            CancelableTask task = new CancelableTask(() =>
            {
                _mingApp.IndicateBusy(Properties.Resources.Status_Deleting);
                try
                {
                    MongoUtilities.Create(cnnInfo)[col.DatabaseName].DropCollection(col.CollectionName);
                }
                catch { }
                _mingApp.RefreshTreeViewItem(node.Parent);
                _mingApp.IndicateIdle();
            }, null);
            task.Execute();
        }

        public void DeleteDatabase(ConnectionInfo cnnInfo, MingTreeViewItem node)
        {
            if (node == null)
            {
                return;
            }

            var db = node.Data as DatabaseNode;
            if (db == null)
            {
                return;
            }

            MessageBox confirm = new MessageBox();
            if (!confirm.ShowConfirm(_mingApp.MainWindow, string.Format(Properties.Resources.Confirm_DeleteDatabase, db.DatabaseName)))
            {
                return;
            }

            CancelableTask task = new CancelableTask(() =>
            {
                _mingApp.IndicateBusy(Properties.Resources.Status_Deleting);
                try
                {
                    MongoUtilities.Create(cnnInfo).DropDatabase(db.DatabaseName);
                }
                catch { }
                _mingApp.RefreshTreeViewItem(node.Parent);
                _mingApp.IndicateIdle();
            }, null);
            task.Execute();
        }

        void BusyStateChanged(bool isBusy, string message)
        {
            if (isBusy)
            {
                _mingApp.IndicateBusy(message);
            }
            else
            {
                _mingApp.IndicateIdle();
            }
        }

        public IEnumerable<string> GetActiveMenuKeys(MingTreeViewItem activeNode)
        {
            var items = new List<string>();

            if (activeNode == null)
            {
                return items;
            }

            if (activeNode.Data is DatabaseNode || activeNode.Data is CollectionsNode || activeNode.Data is CollectionsEmptyNode)
            {
                if (!(activeNode.Data is DatabasesEmptyNode))
                {
                    items.Add(Menus.NewCollection);
                    if (!(activeNode.Data is CollectionIndexesEmptyNode))
                    {
                        items.Add(Menus.CompactCollections);
                    }
                }
                items.Add(Menus.NewDatabase);
                items.Add(Menus.OpenConsole);
                items.Add(Menus.SystemStatus);
            }
            else if (activeNode.Data is TreeViewRootNodeData)
            {
                var rootNodeData = activeNode.Data as TreeViewRootNodeData;
                if (rootNodeData != null)
                {
                    if (rootNodeData.ConnectionInfo.ServiceId == PluginId)
                    {
                        items.Add(Menus.NewDatabase);
                        items.Add(Menus.CompactCollections);
                        items.Add(Menus.WatchLogs);
                        items.Add(Menus.CopyCollections);
                        items.Add(Menus.OpenConsole);
                        items.Add(Menus.SystemStatus);
                        items.Add(Menus.DeleteColsDBs);
                    }
                }
            }
            if (activeNode.Data is ReplicaSetMemberNode)
            {
                items.Add(Menus.NewDatabase);
                items.Add(Menus.CompactCollections);
                items.Add(Menus.WatchLogs);
                items.Add(Menus.CopyCollections);
                items.Add(Menus.OpenConsole);
                items.Add(Menus.SystemStatus);
                items.Add(Menus.DeleteColsDBs);
            }
            if (activeNode.Data is CollectionNode)
            {
                items.Add(Menus.ViewCollection);
                items.Add(Menus.DeleteCollection);
                items.Add(Menus.RenameCollection);
                items.Add(Menus.CopyCollection);
                items.Add(Menus.CopyCollections);
                items.Add(Menus.ManageIndexes);
            }
            if (activeNode.Data is CollectionsNode)
            {
                items.Add(Menus.CopyCollections);
                items.Add(Menus.OpenConsole);
                items.Add(Menus.SystemStatus);
                items.Add(Menus.DeleteColsDBs);
            }
            if (activeNode.Data is DatabaseNode)
            {
                items.Add(Menus.DeleteDatabase);
                items.Add(Menus.EvaluateJavaScript);
                items.Add(Menus.CopyCollections);
                items.Add(Menus.OpenConsole);
                items.Add(Menus.SystemStatus);
                items.Add(Menus.DeleteColsDBs);
            }
            if (activeNode.Data is CollectionIndexesNode)
            {
                items.Add(Menus.ManageIndexes);
            }
            if (activeNode.Data is CollectionIndexNode)
            {
                items.Add(Menus.ManageIndexes);
            }

            return items;
        }

        public IMingMenuClient MenuClient
        {
            get { return this; }
        }

        public void CopyTreeviewNode(MingTreeViewItem node, ConnectionInfo cnnInfo)
        {
            var data = node.Data;
            string str = null;
            if (data is TreeViewRootNodeData || data is ReplicaSetMemberNode)
            {
                str = string.Format("{0}:{1}/?safe=true;slaveOk=true", cnnInfo.Host, cnnInfo.Port);
            }
            if (data is DatabaseNode)
            {
                str = string.Format("{0}:{1}/{2}?safe=true;slaveOk=true", cnnInfo.Host, cnnInfo.Port, (data as DatabaseNode).DatabaseName);
            }
            if (str != null)
            {
                System.Windows.Clipboard.SetText(str);
            }
        }
    }
}
