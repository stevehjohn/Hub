using MingPluginInterfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MingMongoPlugin.Dialogs
{
    internal class TreeViewItem : INotifyPropertyChanged
    {
        private string _text;
        private BitmapImage _icon;
        private bool _selected;
        private bool _isDatabase;
        private string _database;

        private readonly ObservableCollection<object> _items;

        public TreeViewItem(string text, BitmapImage icon)
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
    
    internal class DeleteCollectionsAndDatabases
    {
        private readonly ConnectionInfo _connection;

        private readonly DeleteCollectionsAndDatabasesDialog _dialog;

        private readonly Window _mainWindow;

        public DeleteCollectionsAndDatabases(Window mainWindow, ConnectionInfo connection)
        {
            _connection = connection;
            _mainWindow = mainWindow;

            _dialog = new DeleteCollectionsAndDatabasesDialog();
            _dialog.Owner = mainWindow;
        }

        public void ShowDialog()
        {
            LoadSettings();

            var task = new CancelableTask(PopulateSourceTreeView, null);
            task.Execute();

            var ret = _dialog.ShowDialog();

            SaveSettings();
        }

        private void PopulateSourceTreeView()
        {
            try
            {
                SourceTreeViewItem selectedCollection = null;
                var cnn = MongoUtilities.Create(_connection);
                var dbs = cnn.GetDatabaseNames();
                foreach (var db in dbs)
                {
                    var cols = cnn.GetDatabase(db).GetCollectionNames().Where(item => !item.StartsWith("system."));
                    if (cols.Count() > 0)
                    {
                        var treeDB = new SourceTreeViewItem(db, Utilities.BitmapImageFromBitmap(Properties.Resources.database));
                        treeDB.IsDatabase = true;
                        _dialog.Dispatcher.Invoke(new Action(() => _dialog.SourceTree.Items.Add(treeDB)));
                        cols.ToList().ForEach(col =>
                        {
                            var treeCol = new SourceTreeViewItem(col, Utilities.BitmapImageFromBitmap(Properties.Resources.collection));
                            treeCol.Database = db;
                            //treeCol.PropertyChanged += TreeItemSelectedChanged;
                            _dialog.Dispatcher.Invoke(new Action(() => treeDB.Items.Add(treeCol)));
                        });
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
        }

        private void LoadSettings()
        {
            var val = Properties.Settings.Default.DeleteCollectionsDBsWidth;
            if (val > 0) _dialog.Width = val;

            val = Properties.Settings.Default.DeleteCollectionsDBsHeight;
            if (val > 0) _dialog.Height = val;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.DeleteCollectionsDBsWidth = (int)_dialog.Width;
            Properties.Settings.Default.DeleteCollectionsDBsHeight = (int)_dialog.Height;
            Properties.Settings.Default.Save();
        }
    }
}
