using MingMongoPlugin.Mongo;
using MingMongoPlugin.TabDocuments.UserControls;
using MingPluginInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MingMongoPlugin.TabDocuments
{
    internal class SortField
    {
        public string Name { get; set; }
        public bool Ascending { get; set; }
        public string Arrow
        {
            get
            {
                if (Ascending)
                {
                    return "5";
                }
                else
                {
                    return "6";
                }
            }
        }
    }

    internal class FilterField
    {
        public string Name { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }

        public string Description
        {
            get
            {
                return string.Format("{0} {1} {2}", Name, Operator, Value);
            }
        }
    }

    internal class CollectionView : ITabDocument
    {
        public event BusyStateChangedEventHandler BusyStateChanged;
        public event MenuStateChangedEventHandler MenuStateChanged;

        private readonly CollectionViewControl _control;
        private readonly ConnectionInfo _cnn;
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly Pager _pager;
        private int _nextItemId;

        private ObservableCollection<MongoDocumentProperty> _collectionData;
        private List<string> _sortFieldsTemp;
        private ObservableCollection<string> _sortFields;
        private Dictionary<string, BsonType> _sortFieldsInfo;

        private ObservableCollection<SortField> _sortOn;
        private ObservableCollection<FilterField> _filterOn;

        private int _instanceId;

        public int InstanceId
        {
            set
            {
                _instanceId = value;
            }
        }

        public CollectionView(ConnectionInfo cnn, string database, string collection)
        {
            _control = new CollectionViewControl();
            _control.PropertyExpanderClicked += PropertyExpanderClicked;
            _control.RefreshClicked += RefreshClicked;
            _control.MainListView.SelectionChanged += MainListViewSelectionChanged;
            _control.PreviewKeyUp += PreviewKeyUp;
            _cnn = cnn;
            _collectionName = collection;
            _databaseName = database;

            _pager = new Pager();
            _control.Pager.DataContext = _pager;

            _collectionData = new ObservableCollection<MongoDocumentProperty>();
            _control.MainListView.ItemsSource = _collectionData;

            _sortFields = new ObservableCollection<string>();
            _sortFieldsTemp = new List<string>();
            _sortFieldsInfo = new Dictionary<string, BsonType>();

            _control.SortFields.ItemsSource = _sortFields;
            _control.FilterFields.ItemsSource = _sortFields;

            _sortOn = new ObservableCollection<SortField>();
            _control.SortFieldList.ItemsSource = _sortOn;

            _filterOn = new ObservableCollection<FilterField>();
            _control.FilterFieldList.ItemsSource = _filterOn;

            _control.AddSortClicked += AddSortClicked;
            _control.SortRemoveButtonClicked += SortRemoveButtonClicked;
            _control.AddFilterClicked += AddFilterClicked;
            _control.FilterRemoveButtonClicked += FilterRemoveButtonClicked;

            StartScanCollectionForProperties();
        }

        void PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                ShowData();
                StartScanCollectionForProperties();
            }
        }

        void MainListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MenuStateChanged(Menus.DeleteDocument, ((ListView)sender).SelectedItems.Count > 0, _instanceId);
        }

        private void AddFilterClicked()
        {
            if (_control.FilterFields.SelectedIndex < 0)
                return;
            var field = _control.FilterFields.SelectedItem as string;

            if (_filterOn.Select(item => item.Name).Contains(field))
                return;

            if (string.IsNullOrWhiteSpace(_control.FilterValue.Text))
                return;

            var filterField = new FilterField();
            filterField.Name = field;
            filterField.Operator = (_control.FilterOperator.SelectedValue as ContentControl).Content.ToString();
            filterField.Value = _control.FilterValue.Text.Trim();
            _filterOn.Add(filterField);
            _control.FilterValue.Text = "";
            ShowData();
        }

        private void FilterRemoveButtonClicked(string filterProperty)
        {
            var filter = _filterOn.Where(item => item.Name == filterProperty).First();
            _control.FilterValue.Text = filter.Value;
            _filterOn.Remove(filter);
            ShowData();
        }

        private void AddSortClicked()
        {
            if (_control.SortFields.SelectedIndex < 0)
                return;
            var field = _control.SortFields.SelectedItem as string;

            if (_sortOn.Select(item => item.Name).Contains(field))
                return;

            var sortField = new SortField();
            sortField.Name = field;
            sortField.Ascending = _control.SortDirectionImage.Tag.ToString() == "asc";
            _sortOn.Add(sortField);
            ShowData();
        }

        private void SortRemoveButtonClicked(string sortProperty)
        {
            _sortOn.Remove(_sortOn.Where(item => item.Name == sortProperty).First());
            ShowData();
        }

        private void StartScanCollectionForProperties()
        {
            _sortFields.Clear();
            _sortFieldsInfo.Clear();
            _sortFields.Add(Properties.Resources.CollectionView_Loading);
            _control.AddSort.IsEnabled = false;
            _control.SortFields.SelectedIndex = 0;
            _control.SortFields.IsEnabled = false;
            _control.AddFilter.IsEnabled = false;
            _control.FilterFields.SelectedIndex = 0;
            _control.FilterFields.IsEnabled = false;
            var scanTask = new CancelableTask(ScanCollectionForProperties, null);
            scanTask.Execute();
        }

        private void ScanCollectionForProperties()
        {
            try
            {
                var analyser = new CollectionPropertyAnalyser(
                    MongoUtilities.Create(_cnn),
                    _databaseName,
                    _collectionName);

                _sortFieldsTemp.Clear();

                var allProperties = analyser.GetAllProperties();
                _sortFieldsTemp.AddRange(allProperties.Select(item => item.FullName).ToList());
                allProperties.ToList().ForEach(item => 
                    {
                        if (!_sortFieldsInfo.ContainsKey(item.FullName)) 
                        {
                            _sortFieldsInfo.Add(item.FullName, item.Type);
                        }
                    });

                _control.Dispatcher.Invoke(new Action(() => ShowScanResults()));
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        private void ShowScanResults()
        {
            try
            {
                _sortFields.Clear();
                _sortFieldsTemp.OrderBy(item => item).ToList().ForEach(item => _sortFields.Add(item));
                _control.SortFields.IsEnabled = _sortFields.Count > 0;
                _control.FilterFields.IsEnabled = _sortFields.Count > 0;
                _control.AddSort.IsEnabled = _sortFields.Count > 0;
                _control.AddFilter.IsEnabled = _sortFields.Count > 0;
                if (_sortFields.Count > 0)
                {
                    _control.SortFields.SelectedIndex = 0;
                    _control.FilterFields.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        /*
        private void ScanItemProperties(BsonElement[] items, string path)
        {
            foreach (var item in items)
            {
                string fullName;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    fullName = string.Format("{0}.{1}", path, item.Name);
                }
                else
                {
                    fullName = item.Name;
                }
                if (item.Value.BsonType != BsonType.Document && item.Value.BsonType != BsonType.Array)
                {
                    if (!_sortFieldsTemp.Contains(fullName))
                    {
                        _sortFieldsTemp.Add(fullName);
                        _sortFieldsInfo.Add(fullName, item.Value.BsonType);
                    }
                }
                if (item.Value.BsonType == BsonType.Document)
                {
                    ScanItemProperties(item.Value.AsBsonDocument.ToArray(), fullName);
                }
            }
        }*/

        private void PagerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ShowAsHierarchy();
        }

        public UserControl Control
        {
            get { return _control; }
        }

        public string Title
        {
            get { return string.Format(Properties.Resources.CollectionView_Title, _collectionName); }
        }

        public string Description
        {
            get { return string.Format(Properties.Resources.CollectionView_Description, _cnn.Host, _cnn.Port, _databaseName, _collectionName); } 
        }

        public bool CloseRequested()
        {
            return true;
        }

        public void ShowData()
        {
            try
            {
                ShowAsHierarchy();
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
        }

        private void ShowAsHierarchy()
        {
            var view = _control.MainListView.View as GridView;

            _collectionData.Clear();
            _pager.PropertyChanged -= PagerPropertyChanged;
            var loadTask = new CancelableTask<MongoCursor<BsonDocument>>(
                () => 
                LoadData(), 
                result => DataLoaded(result));
            if (BusyStateChanged != null) BusyStateChanged(true, Properties.Resources.CollectionView_Loading);
            loadTask.Execute();
        }

        private MongoCursor<BsonDocument> LoadData()
        {
            try
            {
                var db = MongoUtilities.Create(_cnn).GetDatabase(_databaseName);
                var col = db.GetCollection(_collectionName);
                _pager.TotalItems = (int)col.Count();
                if (_pager.TotalItems == 0)
                {
                    return null;
                }
                var queries = new List<IMongoQuery>();
                _filterOn.ToList().ForEach(property =>
                {
                    IMongoQuery subQuery;
                    BsonValue value;
                    if (!MongoUtilities.TryConvertStringToBsonType(
                        _sortFieldsInfo[property.Name], property.Value, out value))
                    {
                        value = new BsonString(property.Value);
                    }
                    switch (property.Operator)
                    {
                        case "!=":
                            subQuery = Query.NE(property.Name, value);
                            break;
                        case ">=":
                            subQuery = Query.GTE(property.Name, value);
                            break;
                        case "<=":
                            subQuery = Query.LTE(property.Name, value);
                            break;
                        case ">":
                            subQuery = Query.GT(property.Name, value);
                            break;
                        case "<":
                            subQuery = Query.LT(property.Name, value);
                            break;
                        case "like":
                            subQuery = Query.Matches(property.Name, new BsonRegularExpression("(?i)" + property.Value));
                            break;
                        case "in":
                        case "!in":
                            var values = new List<BsonValue>();
                            property.Value.Split(',').ToList().ForEach(item =>
                            {
                                BsonValue val = null;
                                if (!MongoUtilities.TryConvertStringToBsonType(_sortFieldsInfo[property.Name], item.Trim(), out val))
                                {
                                    val = new BsonString(item.Trim());
                                }
                                values.Add(val);
                            });
                            if (property.Operator == "in")
                            {
                                subQuery = Query.In(property.Name, values);
                            }
                            else
                            {
                                subQuery = Query.NotIn(property.Name, values);
                            }
                            break;
                        default:
                            subQuery = Query.EQ(property.Name, value);
                            break;
                    }
                    queries.Add(subQuery);
                });
                IMongoQuery query;
                if (queries.Count > 0)
                {
                    query = Query.And(queries);
                }
                else
                {
                    query = null;
                }
                var cur = col.Find(query).SetSkip(_pager.FirstItemIndex).SetLimit(_pager.PageSize);
                SortByBuilder sort = new SortByBuilder();
                _sortOn.ToList().ForEach(property =>
                {
                    if (property.Ascending)
                        sort = sort.Ascending(property.Name);
                    else
                        sort = sort.Descending(property.Name);
                });
                cur.SetSortOrder(sort);
                return cur;
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }

            return null;
        }

        private void DataLoaded(MongoCursor<BsonDocument> data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }

                bool alt = true;
                _nextItemId = 0;
                data.ToList().ForEach(item => { AddItems(item.ToArray(), 0, alt, null, item["_id"].AsBsonValue); alt = !alt; });
                _pager.PropertyChanged += PagerPropertyChanged;
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
            finally
            {
                if (BusyStateChanged != null) BusyStateChanged(false, null);
            }
        }

        private void AddItems(BsonElement[] elements, int level, bool alt, MongoDocumentProperty parent, BsonValue objectId, int? arrayIndex = null)
        {
            var orderedElements = elements.OrderBy(element => element.Name);

            foreach (var item in orderedElements)
            {
                var mdp = new MongoDocumentProperty
                {
                    AlternateRow = alt,
                    ArrayIndex = arrayIndex,
                    Depth = level * 20,
                    DocumentObjectId = objectId,
                    Expanded = true,
                    ExpanderVisibility = Visibility.Hidden,
                    FullPath = parent == null ? item.Name : string.Format("{0}{1}{2}", parent.FullPath, arrayIndex == null ? "." : "", item.Name),
                    Id = _nextItemId++,
                    Key = item.Name,
                    TextBoxVisibility = Visibility.Visible,
                    Type = item.Value.BsonType,
                    Value = null,
                    Visible = Visibility.Visible
                };
                if (level == 0 && item.Name == "_id")
                {
                    mdp.ReadOnly = true;
                }

                if (parent != null)
                {
                    parent.AddChild(mdp);
                }

                switch (item.Value.BsonType)
                {
                    case BsonType.Document:
                        mdp.ExpanderVisibility = Visibility.Visible;
                        mdp.TextBoxVisibility = Visibility.Hidden;
                        mdp.ReadOnly = true;
                        _collectionData.Add(mdp);
                        AddItems(item.Value.AsBsonDocument.ToArray(), level + 1, alt, mdp, objectId);
                        break;
                    case BsonType.Array:
                        mdp.ExpanderVisibility = Visibility.Visible;
                        mdp.TextBoxVisibility = Visibility.Hidden;
                        mdp.ReadOnly = true;
                        _collectionData.Add(mdp);
                        var idx = 0;
                        foreach (var arrayItem in item.Value.AsBsonArray)
                        {
                            AddItems(new[] { new BsonElement(string.Format("[{0}]", idx), arrayItem) }, level + 1, alt, mdp, objectId, idx);
                            idx++;
                        }
                        break;
                    case BsonType.Binary:
                        mdp.Value = Convert.ToBase64String((byte[]) item.Value);
                        mdp.PropertyChanged += DocumentPropertyChanged;
                        _collectionData.Add(mdp);
                        break;
                    default:
                        mdp.Value = item.Value.ToString();
                        mdp.PropertyChanged += DocumentPropertyChanged;
                        _collectionData.Add(mdp);
                        break;
                }
            }
        }

        private void DocumentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                var doc = sender as MongoDocumentProperty;
                BsonValue bsonValue;
                if (MongoUtilities.TryConvertStringToBsonType(doc.Type, doc.Value, out bsonValue))
                {
                    var updateTask = new CancelableTask<MongoDocumentProperty>(
                        () => UpdateDocument(doc), result => DocumentUpdated(result));
                    doc.IsInError = false;
                    doc.IsUpdating = true;
                    updateTask.Execute();
                }
                else
                {
                    doc.IsInError = true;
                }
            }
        }

        private MongoDocumentProperty UpdateDocument(MongoDocumentProperty doc)
        {
            var db = MongoUtilities.Create(_cnn).GetDatabase(_databaseName);
            var col = db.GetCollection(_collectionName);
            var query = Query.EQ("_id", doc.DocumentObjectId);

            try
            {
                var document = GetBsonDocumentFromCollection(doc.DocumentObjectId);
                col.Save(document);
            }
            catch
            {
                doc.IsInError = true;
            }
            return doc;
        }

        private BsonDocument GetBsonDocumentFromCollection(BsonValue id)
        {
            BsonDocument doc = new BsonDocument();

            var roots = _collectionData.Where(item => item.DocumentObjectId == id && item.Parent == null);

            roots.ToList().ForEach(item => doc.Add(item.Key, GetBsonValue(item)));

            return doc;
        }

        private BsonValue GetBsonValue(MongoDocumentProperty property)
        {
            switch (property.Type)
            {
                case BsonType.Array:
                    BsonArray arr = new BsonArray();
                    property.Children.ToList().ForEach(
                        item => arr.Add(GetBsonValue(item)));
                    return arr;
                case BsonType.Document:
                    BsonDocument doc = new BsonDocument();
                    property.Children.ToList().ForEach(
                        item => doc.Add(item.Key, GetBsonValue(item)));
                    return doc;
                default:
                    BsonValue value;
                    if (MongoUtilities.TryConvertStringToBsonType(property.Type, property.Value, out value))
                    {
                        return value;
                    }
                    return null;
            }
        }

        private void DocumentUpdated(MongoDocumentProperty doc)
        {
            doc.Updated();
        }

        private void PropertyExpanderClicked(MongoDocumentProperty documentProperty)
        {
            var visibility = Visibility.Visible;
            if (! documentProperty.Expanded)
            {
                visibility = Visibility.Collapsed;
            }
            SetChildrensVisibility(documentProperty, visibility);
        }

        private void SetChildrensVisibility(MongoDocumentProperty documentProperty, Visibility visibility)
        {
            documentProperty.Children.ToList().ForEach(item => { item.Visible = visibility; SetChildrensVisibility(item, visibility); });
        }

        private void RefreshClicked()
        {
            ShowData();
            StartScanCollectionForProperties();
        }

        public void MenuItemClicked(string menuKey)
        {
            switch (menuKey)
            {
                case Menus.DeleteDocument:
                    DeleteSelectedDocument();
                    break;
            }
        }

        private void DeleteSelectedDocument()
        {
            var item = _control.MainListView.SelectedItem as MongoDocumentProperty;
            var task = new CancelableTask(() => DeleteDocument(item.DocumentObjectId), null);
            task.Execute();
        }

        private void DeleteDocument(BsonValue id)
        {
            try
            {
                if (BusyStateChanged != null)
                {
                    BusyStateChanged(true, Properties.Resources.DeleteDocument_Deleting);
                }
                MongoUtilities.Create(_cnn).GetDatabase(_databaseName).GetCollection(_collectionName).Remove(Query.EQ("_id", id));
            }
            catch (Exception ex)
            {
                Utilities.LogException(ex);
            }
            finally
            {
                BusyStateChanged(false, null);
                _control.Dispatcher.Invoke(new Action(() => 
                    {
                        ShowData();
                        StartScanCollectionForProperties();
                    }));
            }
        }
    }
}
