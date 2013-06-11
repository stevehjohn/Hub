using MingControls.Controls;
using MingControls.Extensions;
using MingMongoPlugin.MongoFunctions;
using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MingMongoPlugin.Dialogs
{
    internal class SourceTreeViewItem : INotifyPropertyChanged
    {
        private string _text;
        private BitmapImage _icon;
        private bool _selected;
        private bool _isDatabase;
        private string _database;

        private readonly ObservableCollection<object> _items;

        public SourceTreeViewItem(string text, BitmapImage icon)
        {
            _text = text;
            _icon = icon;
            _items = new ObservableCollection<object>();
        }

        public BitmapImage Icon
        {
            get
            {
                return _icon;
            }
        }

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                if (_isDatabase)
                {
                    _items.ToList().ForEach(item => ((SourceTreeViewItem)item).Selected = _selected);
                }
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
            }
        }

        public bool IsDatabase
        {
            get
            {
                return _isDatabase;
            }
            set
            {
                _isDatabase = value;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }

        public ObservableCollection<object> Items 
        { 
            get 
            { 
                return _items; 
            } 
        }

        public string Database
        {
            get
            {
                return _database;
            }
            set
            {
                _database = value;
            }
        }

        public bool IsExpanded { get { return true; } }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    internal class TargetConnection
    {
        private readonly string _description;
        private readonly ConnectionInfo _cnnInfo;

        public TargetConnection(string description, ConnectionInfo cnnInfo)
        {
            _description = description;
            _cnnInfo = cnnInfo;
        }

        public string Description
        {
            get { return _description; }
        }

        public ConnectionInfo ConnectionInfo
        {
            get { return _cnnInfo; }
        }
    }

    internal class CopyTargetDefinition
    {
        private readonly string _sourceCollection;
        private readonly string _sourceDatabase;

        public CopyTargetDefinition(string sourceCollection, string sourceDatabase)
        {
            _sourceCollection = sourceCollection;
            _sourceDatabase = sourceDatabase;
        }

        public string SourceCollection
        {
            get { return _sourceCollection; }
        }

        public string SourceDatabase
        {
            get { return _sourceDatabase; }
        }

        public string SourceCollectionFullName
        {
            get { return string.Format("{0}.{1}", _sourceDatabase, _sourceCollection); }
        }

        public string TargetDatabase { get; set; }

        public string NewCollectionName { get; set; }

        public ObservableCollection<string> TargetDatabases { get; set; }
    }

    internal class CopyCollections
    {
        private readonly Window _mainWindow;
        private readonly CopyCollectionsDialog _dialog;
        private readonly ConnectionInfo _sourceConnection;
        private readonly ObservableCollection<TargetConnection> _targetConnections;
        private readonly ObservableCollection<CopyTargetDefinition> _targetCollections;
        private readonly ObservableCollection<string> _targetDatabases;

        private string _databaseName;
        private string _collectionName;

        private CancelableTask<string> _prepareTask;
        private ProgressDialog _prepareProgress;

        private TargetConnection _targetConnection;

        private List<CopyCollectionDefinition> _result;

        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }
            set
            {
                _databaseName = value;
            }
        }

        public string CollectionName
        {
            get
            {
                return _collectionName;
            }
            set
            {
                _collectionName = value;
            }
        }

        public IEnumerable<CopyCollectionDefinition> CollectionsToCopy
        {
            get
            {
                return _result;
            }
        }

        public CopyCollections(Window mainWindow, ConnectionInfo sourceConnection, IEnumerable<ConnectionInfo> connections)
        {
            _mainWindow = mainWindow;
            _dialog = new CopyCollectionsDialog();
            _dialog.Owner = _mainWindow;
            _sourceConnection = sourceConnection;

            _targetDatabases = new ObservableCollection<string>();

            _targetConnections = new ObservableCollection<TargetConnection>();
            connections.ToList().ForEach(item => _targetConnections.Add(
                new TargetConnection(
                    string.IsNullOrWhiteSpace(item.Name) ? string.Format("{0}:{1}", item.Host, item.Port) : item.Name, 
                    item)));
            _dialog.TargetServer.ItemsSource = _targetConnections;
            _dialog.TargetServer.SelectionChanged += TargetServerSelectionChanged;
            _dialog.TargetServer.SelectedIndex = 0;

            _targetCollections = new ObservableCollection<CopyTargetDefinition>();
            _dialog.TargetCollections.ItemsSource = _targetCollections;

            _dialog.SourceServer.Text = string.Format("{0}:{1}", _sourceConnection.Host, _sourceConnection.Port);

            _dialog.TargetDatabaseLostFocus += TargetDatabaseLostFocus;
            _dialog.CopyButton.Click += CopyButtonClick;
        }

        private void CopyButtonClick(object sender, RoutedEventArgs e)
        {
            if (ValidatePreCopy())
            {
                _prepareProgress = new ProgressDialog();
                _prepareProgress.Title = Properties.Resources.CopyCollections_PrepareTitle;
                _prepareProgress.Owner = _dialog;

                _targetConnection = _dialog.TargetServer.SelectedItem as TargetConnection;

                _prepareTask = new CancelableTask<string>(() => PrepareToCopy(), PrepareToCopyComplete);
                _prepareTask.Execute();
                if (!(bool)_prepareProgress.ShowDialog())
                {
                    _prepareProgress = null;
                    if (_prepareTask != null)
                    {
                        _prepareTask.Cancel();
                    }
                }
            }
        }

        private bool ValidatePreCopy()
        {
            if (_targetCollections.Count == 0)
            {
                var msg = new MingPluginInterfaces.Forms.MessageBox();
                msg.ShowMessage(_dialog, Properties.Resources.CopyCollections_NoneSelected, Properties.Resources.CopyCollections_ErrorMessageTitle);
                return false;
            }

            var ok = true;

            _targetCollections.ToList().ForEach(item =>
                {
                    if (item.NewCollectionName.Trim().Length == 0 || item.TargetDatabase.Trim().Length == 0) ok = false;
                });

            if (!ok)
            {
                var msg = new MingPluginInterfaces.Forms.MessageBox();
                msg.ShowMessage(_dialog, Properties.Resources.CopyCollections_TargetError, Properties.Resources.CopyCollections_ErrorMessageTitle);
            }

            return ok;
        }

        private string PrepareToCopy()
        {
            var cnn = MongoUtilities.Create(_targetConnection.ConnectionInfo);

            // TODO: Check cnn.Instance.IsPrimary (had some issues with this though)

            var existingCols = new List<string>();
            var targets = _targetCollections.ToList();
            var targetDBs = cnn.GetDatabaseNames();
            targets.ForEach(item =>
                {
                    if (targetDBs.Contains(item.TargetDatabase))
                    {
                        if (cnn.GetDatabase(item.TargetDatabase).GetCollectionNames().Contains(item.NewCollectionName))
                        {
                            existingCols.Add(string.Format("{0}.{1}", item.TargetDatabase, item.NewCollectionName));
                        }
                    }
                });

            if (existingCols.Count > 0)
            {
                var names = new StringBuilder();
                existingCols.ForEach(item => names.Append(string.Format("{0}\n", item)));
                return string.Format(Properties.Resources.CopyCollections_TargetsExists, names);
            }

            var badNames = new List<string>();
            string msg;

            foreach (var item in targets)
            {
                if (!cnn.IsDatabaseNameValid(item.TargetDatabase, out msg))
                {
                    if (!badNames.Contains(item.TargetDatabase))
                    {
                        badNames.Add(item.TargetDatabase);
                    }
                }
                else
                {
                    if (!cnn.GetDatabase(item.TargetDatabase).IsCollectionNameValid(item.NewCollectionName, out msg) || item.NewCollectionName.Contains('$'))
                    {
                        if (!badNames.Contains(item.NewCollectionName))
                        {
                            badNames.Add(item.NewCollectionName);
                        }
                    }
                }
            }

            if (badNames.Count > 0)
            {
                var names = new StringBuilder();
                badNames.ForEach(item => names.Append(string.Format("{0}\n", item)));
                return string.Format(Properties.Resources.CopyCollections_InvalidNames, names);
            }

            return null;
        }

        private void PrepareToCopyComplete(string result)
        {
            _prepareTask = null;
            if (_prepareProgress != null)
            {
                _prepareProgress.Hide();
                _prepareProgress = null;
            }

            if (result != null)
            {
                var msg = new MingPluginInterfaces.Forms.MessageBox();
                msg.ShowMessage(_dialog, result, Properties.Resources.CopyCollections_ErrorMessageTitle);
                return;
            }

            _targetCollections.ToList().ForEach(item =>
                _result.Add(new CopyCollectionDefinition(
                    new CollectionDefinition(_sourceConnection, item.SourceDatabase, item.SourceCollection),
                    new CollectionDefinition(_targetConnection.ConnectionInfo, item.TargetDatabase, item.NewCollectionName))));

            _dialog.DialogResult = true;
            _dialog.Hide();
        }

        private void LoadSettings()
        {
            int val;

            // TODO: Dialog width and height and splitter position?

            var grid = _dialog.TargetCollections.View as GridView;

            val = Properties.Settings.Default.CopyCollectionsWidth;
            if (val > 0) _dialog.Width = val;

            val = Properties.Settings.Default.CopyCollectionsHeight;
            if (val > 0) _dialog.Height = val;

            val = Properties.Settings.Default.CopyCollectionsColumn1;
            if (val > 0) grid.Columns[0].Width = val;
            
            val = Properties.Settings.Default.CopyCollectionsColumn2;
            if (val > 0) grid.Columns[1].Width = val;
            
            val = Properties.Settings.Default.CopyCollectionsColumn3;
            if (val > 0) grid.Columns[2].Width = val;

            val = Properties.Settings.Default.CopyCollectionsLeftPaneWidth;
            if (val > 0) _dialog.MainGrid.ColumnDefinitions[0].Width = new GridLength(val);
        }

        private void SaveSettings()
        {
            var grid = _dialog.TargetCollections.View as GridView;
            Properties.Settings.Default.CopyCollectionsWidth = (int)_dialog.Width;
            Properties.Settings.Default.CopyCollectionsHeight = (int)_dialog.Height;
            Properties.Settings.Default.CopyCollectionsColumn1 = (int)grid.Columns[0].Width.ToIntOr0IfNaN();
            Properties.Settings.Default.CopyCollectionsColumn2 = (int)grid.Columns[1].Width.ToIntOr0IfNaN();
            Properties.Settings.Default.CopyCollectionsColumn3 = (int)grid.Columns[2].Width.ToIntOr0IfNaN();
            Properties.Settings.Default.CopyCollectionsLeftPaneWidth = (int)_dialog.MainGrid.ColumnDefinitions[0].Width.Value;
            Properties.Settings.Default.Save();
        }

        private void TargetDatabaseLostFocus(string value)
        {
            if (!_targetDatabases.Contains(value))
            {
                _targetDatabases.Add(value);
            }
        }

        private void TargetServerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _targetDatabases.Clear();
            if (e.AddedItems.Count < 1)
            {
                return;
            }

            var task = new CancelableTask(() => PopulateTargetDatabases(e.AddedItems[0] as TargetConnection), null);
            task.Execute();
        }

        private void PopulateTargetDatabases(TargetConnection target)
        {
            _dialog.Dispatcher.Invoke(new Action(() => _dialog.Status.Text = Properties.Resources.CopyCollections_LoadingTargets));
            try
            {
                var dbs = MongoUtilities.Create(target.ConnectionInfo).GetDatabaseNames().OrderBy(item => item);
                _dialog.Dispatcher.Invoke(new Action(() => dbs.ToList().ForEach(item => { if (!_targetDatabases.Contains(item)) _targetDatabases.Add(item); })));
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
            _dialog.Dispatcher.Invoke(new Action(() => _dialog.Status.Text = string.Empty));
        }

        public bool ShowDialog()
        {
            LoadSettings();

            var task = new CancelableTask(PopulateSourceTreeView, null);
            task.Execute();

            _result = new List<CopyCollectionDefinition>();
            var ret = (bool) _dialog.ShowDialog();

            SaveSettings();

            return ret;
        }

        private void PopulateSourceTreeView()
        {
            _dialog.Dispatcher.Invoke(new Action(() => _dialog.Status.Text = Properties.Resources.CopyCollections_LoadingCollections));
            try
            {
                SourceTreeViewItem selectedCollection = null;
                var cnn = MongoUtilities.Create(_sourceConnection);
                var dbs = cnn.GetDatabaseNames();
                foreach (var db in dbs)
                {
                    var cols = cnn.GetDatabase(db).GetCollectionNames().Where(item => ! item.StartsWith("system."));
                    if (cols.Count() > 0)
                    {
                        var treeDB = new SourceTreeViewItem(db, Utilities.BitmapImageFromBitmap(Properties.Resources.database));
                        treeDB.IsDatabase = true;
                        _dialog.Dispatcher.Invoke(new Action(() => _dialog.SourceTree.Items.Add(treeDB)));
                        cols.ToList().ForEach(col => 
                            {
                                var treeCol = new SourceTreeViewItem(col, Utilities.BitmapImageFromBitmap(Properties.Resources.collection));
                                treeCol.Database = db;
                                treeCol.PropertyChanged += TreeItemSelectedChanged;
                                if (col == _collectionName)
                                {
                                    selectedCollection = treeCol;
                                }
                                _dialog.Dispatcher.Invoke(new Action(() => treeDB.Items.Add(treeCol)));
                            });
                        if (_databaseName == db && string.IsNullOrWhiteSpace(_collectionName))
                        {
                            _dialog.Dispatcher.Invoke(new Action(() => treeDB.Selected = true));
                        }
                    }
                }
                if (selectedCollection != null)
                {
                    _dialog.Dispatcher.Invoke(new Action(() => selectedCollection.Selected = true));
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
            _dialog.Dispatcher.Invoke(new Action(() => _dialog.Status.Text = string.Empty));
        }

        private void TreeItemSelectedChanged(object sender, PropertyChangedEventArgs e)
        {
            var source = sender as SourceTreeViewItem;
            if (source.Selected)
            {
                if (_targetCollections.Where(item => item.SourceCollectionFullName == string.Format("{0}.{1}", source.Database, source.Text)).Count() == 0)
                {
                    var newCol = new CopyTargetDefinition(source.Text, source.Database);
                    newCol.NewCollectionName = source.Text;
                    newCol.TargetDatabase = source.Database;
                    newCol.TargetDatabases = _targetDatabases;
                    _targetCollections.Add(newCol);
                }
            }
            else
            {
                var remove = _targetCollections.Where(item => item.SourceCollectionFullName == string.Format("{0}.{1}", source.Database, source.Text));
                if (remove.Count() > 0)
                {
                    _targetCollections.Remove(remove.First());
                }
            }
        }
    }
}
