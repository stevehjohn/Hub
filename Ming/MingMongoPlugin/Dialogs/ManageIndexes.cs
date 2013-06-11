using MingMongoPlugin.DataObjects;
using MingMongoPlugin.Mongo;
using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace MingMongoPlugin.Dialogs
{
    internal class ManageIndexes
    {
        private readonly ManageIndexesDialog _dialog;
        private readonly Window _mainWindow;
        private readonly ConnectionInfo _connection;
        private readonly string _database;
        private readonly string _collectionName;

        private readonly ObservableCollection<string> _properties;

        public ManageIndexes(Window mainWindow, ConnectionInfo connection, string database, string collectionName)
        {
            _mainWindow = mainWindow;
            _connection = connection;
            _database = database;
            _collectionName = collectionName;

            _dialog = new ManageIndexesDialog();
            _dialog.Owner = _mainWindow;

            _properties = new ObservableCollection<string>();
            _dialog.AvailableProperties.ItemsSource = _properties;

            _dialog.SaveButton.Click += SaveButtonClick;
        }

        public IEnumerable<IndexDescriptor> Indexes
        {
            get
            {
                return _dialog.IndexList.Indexes;
            }
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            _dialog.DialogResult = true;
            _dialog.Hide();
        }

        public bool ShowDialog()
        {
            _dialog.Title = string.Format(Properties.Resources.ManageIndexes_Title, _collectionName);

            LoadSettings();

            StartLoadProperties();
   
            var ret = (bool) _dialog.ShowDialog();

            SaveSettings();

            return ret;
        }

        private void StartLoadProperties()
        {
            var scanTask = new CancelableTask(LoadProperties, null);
            scanTask.Execute();
        }

        private void LoadProperties()
        {
            try
            {
                var analyser = new CollectionPropertyAnalyser(
                    MongoUtilities.Create(_connection),
                    _database,
                    _collectionName);

                var props = analyser.GetAllProperties();

                _mainWindow.Dispatcher.Invoke(new Action(() => PopulateList(props)));                    
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        private void PopulateList(IEnumerable<CollectionProperty> properties)
        {
            properties.ToList().ForEach(
                prop =>
                    _properties.Add(prop.FullName));

            var idxs = MongoUtilities.Create(_connection).GetDatabase(_database).GetCollection(_collectionName).GetIndexes();

            foreach (var idx in idxs)
            {
                var descriptor = new IndexDescriptor { IsSparse = idx.IsSparse, IsUnique = idx.IsUnique };

                var names = idx.Key.Names.ToArray();
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == "_id" && names.Length == 1)
                    {
                        continue;
                    }

                    var prop = new IndexDescriptorProperty { PropertyName = names[i] };
                    if (idx.Key[i].BsonType == MongoDB.Bson.BsonType.String)
                    {
                        prop.IndexType = IndexType.Geospatial;
                    }
                    else
                    {
                        if (idx.Key[i].ToInt32() > 0)
                        {
                            prop.IndexType = IndexType.Ascending;
                        }
                        else
                        {
                            prop.IndexType = IndexType.Descending;
                        }
                    }
                    descriptor.IndexedProperties.Add(prop);
                }

                if (descriptor.IndexedProperties.Count > 0)
                {
                    _dialog.IndexList.AddIndex(descriptor);
                }
            }
        }

        private void LoadSettings()
        {
            int val;

            val = Properties.Settings.Default.ManageIndexesWidth;
            if (val > 0) _dialog.Width = val;

            val = Properties.Settings.Default.ManageIndexesHeight;
            if (val > 0) _dialog.Height = val;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.ManageIndexesWidth = (int)_dialog.Width;
            Properties.Settings.Default.ManageIndexesHeight = (int)_dialog.Height;
        }
    }
}
