using Ming.Controllers;
using Ming.Forms;
using Ming.Infrastructure;
using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows;

namespace Ming
{
    internal class MingApp : ITreeViewContextMenuClient, IMingMenuClient, IMingApp
    {
        private MainWindow _mainWindow;
        private ConnectionsTreeController _connectionsTreeController;
        private MenuController _menuController;
        private int _busyCount = 0;
        private Timer _statusTimer;
        private TabDocumentController _tabDocumentController;
        private ObservableCollection<OperationStatus> _operations = new ObservableCollection<OperationStatus>();

        private const int StatusMessageDuration = 15;

        private int _activeTabId;

        public void Start()
        {
            _mainWindow = new MainWindow();
            _connectionsTreeController = new ConnectionsTreeController(_mainWindow.ConnectionsTree);
            _connectionsTreeController.SelectedItemChanged += new SelectedItemChangedEventHandler(ConnectionsTreeController_SelectedItemChanged);
            _menuController = new MenuController(_mainWindow.MainMenu, _mainWindow.ToolBand);
            _menuController.ActiveTabMenuItemClicked += ActiveTabMenuItemClicked;

            _menuController.AddMenuItems(CreateMenuItems(), this, null);
            _menuController.AddContextMenuKeyProvider(this);

            PluginManager.Instance.Plugins.ToList().ForEach(item => _menuController.AddMenuItems(item.Instance.AllMenuItems, item.Instance.MenuClient, item.Instance.Id));

            _menuController.AddMenuItems(CreateHelpMenu(), this, null);

            _menuController.CreateMenus();

            _statusTimer = new Timer(StatusMessageDuration * 1000);
            _statusTimer.AutoReset = false;
            _statusTimer.Elapsed += new ElapsedEventHandler(StatusTimer_Elapsed);

            _menuController.ActivateToolBarItems();
            _mainWindow.WindowState = SettingsManager.Instance.WindowState;
            _mainWindow.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            Rect r = SettingsManager.Instance.WindowPosition;
            if (r.Width > 0 && r.Height > 0)
            {
                _mainWindow.Left = r.Left;
                _mainWindow.Top = r.Top;
                _mainWindow.Width = r.Width;
                _mainWindow.Height = r.Height;
            }
            _mainWindow.TreeAndContentGrid.ColumnDefinitions[0].Width = new GridLength((double) SettingsManager.Instance.TreeWidth);

            _tabDocumentController = new TabDocumentController(_mainWindow.Documents);

            _mainWindow.OperationsList.ItemsSource = _operations;

            _mainWindow.Show();

            CheckLicensing();

            //ShowNewInfo();
        }

        private void CheckLicensing() 
        {
            var licenceEnforcer = LicenceEnforcerFactory.Instance;

            if (licenceEnforcer.Status == Infrastructure.Interfaces.LicenceStatus.TrialExpired)
            {
                var licenceFormController = new LicenceEntryController(_mainWindow);
                licenceFormController.Show();
                End();
            }
        }

        void ActiveTabMenuItemClicked(string menuKey)
        {
            _tabDocumentController.GetTabDocument(_activeTabId).ITabDocumentInterface.MenuItemClicked(menuKey);
        }

        private void ShowNewInfo()
        {
            var curVer = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            // TODO: Maybe remove this and change the caption on the change log to "welcome" rather than "congratulations"
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.LastVersionRun))
            {
                Properties.Settings.Default.LastVersionRun = curVer;
                Properties.Settings.Default.Save();
                return;
            }

            if (ParseVersionNumber(Properties.Settings.Default.LastVersionRun) == ParseVersionNumber(curVer))
            {
                return;
            }

            var upgraded = new Upgraded();
            upgraded.Owner = _mainWindow;
            upgraded.ShowDialog();

            Properties.Settings.Default.LastVersionRun = curVer;
            Properties.Settings.Default.Save();
        }

        private long ParseVersionNumber(string version)
        {
            long ver = 0;
            if (string.IsNullOrWhiteSpace(version))
            {
                return 0;
            }
            try
            {
                var parts = version.Split('.');
                var ordered = parts.Reverse();
                var bitshift = 0;
                ordered.ToList().ForEach(part => { ver += int.Parse(part) << bitshift; bitshift += 8; });
            }
            catch
            {
                return 0;
            }
            return ver;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveState();
        }

        public void SaveState()
        {
            SettingsManager.Instance.WindowState = _mainWindow.WindowState;
            if (_mainWindow.WindowState != WindowState.Maximized)
            {
                Rect r = new Rect(
                    _mainWindow.Left, _mainWindow.Top,
                    _mainWindow.Width, _mainWindow.Height);
                SettingsManager.Instance.WindowPosition = r;
            }
            SettingsManager.Instance.TreeWidth = (int) _mainWindow.TreeAndContentGrid.ColumnDefinitions[0].Width.Value;
        }

        public void End()
        {
            SaveState();
            _mainWindow.Close();
        }

        public void Connect()
        {
            var controller = new ConnectController(_mainWindow);

            controller.Show();
        }

        public void IndicateBusy(string message)
        {
            _statusTimer.Stop();
            if (message == null)
                message = Properties.Resources.Status_Loading;
            _mainWindow.Dispatcher.Invoke(new Action(() =>
                {
                    _mainWindow.StatusProgress.Visibility = System.Windows.Visibility.Visible;
                    _mainWindow.StatusText.Text = message;
                    _mainWindow.StatusText.Visibility = System.Windows.Visibility.Visible;
                }));
            _busyCount++;
        }

        public void IndicateIdle()
        {
            _busyCount--;
            if (_busyCount == 0)
            {
                _mainWindow.Dispatcher.Invoke(new Action(() =>
                    {
                        _mainWindow.StatusProgress.Visibility = System.Windows.Visibility.Collapsed;
                        _mainWindow.StatusText.Visibility = System.Windows.Visibility.Collapsed;
                    }));
            }
        }

        public void StatusMessage(string message)
        {
            _statusTimer.Stop();
            _mainWindow.StatusText.Text = message;
            _mainWindow.StatusText.Visibility = Visibility.Visible;
            _statusTimer.Start();
        }

        public void ClearStatusMessage()
        {
            _mainWindow.StatusText.Visibility = Visibility.Collapsed;
        }

        void StatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _mainWindow.StatusText.Dispatcher.Invoke(new Action(ClearStatusMessage), null);
        }

        public void Disconnect(ConnectionInfo cnnInfo)
        {
            if (cnnInfo == null)
            {
                return;
            }

            var confirm = new Ming.Forms.MessageBox();
            if (confirm.ShowConfirm(_mainWindow, string.Format(Properties.Resources.Connect_Confirm_Delete_Message,
                string.Format("{0}:{1}", cnnInfo.Host, cnnInfo.Port),
                PluginManager.Instance.GetPluginInstance(cnnInfo.ServiceId).Name)))
            {
                SettingsManager.Instance.DeleteConnection(cnnInfo);
            }
        }

        public IList<object> TreeViewContextMenu(MingTreeViewItem originator)
        {
            var plugin = PluginManager.Instance.GetPluginInfo(_connectionsTreeController.GetNodeConnection(originator).ServiceId);

            return _menuController.GetContextMenu(originator, plugin);
        }

        public const string MenuFile = "MINGAPP_FILE";
        public const string MenuFileAddConnection = "MINGAPP_FILEADDCNN";
        public const string MenuFileDeleteConnection = "MINGAPP_FILEDELCNN";
        public const string MenuFileImport = "MINGAPP_FILEIMP";
        public const string MenuFileExport = "MINGAPP_FILEEXP";
        public const string MenuFileExit = "MINGAPP_FILEEXIT";

        public const string MenuHelp = "MINGAPP_HELP";
        public const string MenuHelpAbout = "MINGAPP_HELPABOUT";

        public const string MenuRefresh = "MINGAPP_REFRESH";

        private IEnumerable<MingMenuItem> CreateMenuItems()
        {
            var items = new List<MingMenuItem>();

            var file = MingMenuItemFactory.CreateMenuItem(
                MenuFile, null, Properties.Resources.Menu_File, MenuContext.MainMenu);
            items.Add(file);

            file.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(
                MenuFileAddConnection, Properties.Resources.server_add, Properties.Resources.Menu_File_Connect, MenuContext.MainMenu | MenuContext.ToolBarContext));
            file.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(
                MenuFileDeleteConnection, Properties.Resources.server_delete, Properties.Resources.Menu_File_Disconnect, MenuContext.MainMenu | MenuContext.ToolBarContext | MenuContext.TreeViewContext));
            file.AddSubMenuItem(MingMenuItemFactory.CreateSeparator(MenuContext.MainMenu));
            file.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(
                MenuFileImport, Properties.Resources.import, Properties.Resources.Menu_File_Import, MenuContext.MainMenu | MenuContext.ToolBarContext));
            file.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(
                MenuFileExport, Properties.Resources.export, Properties.Resources.Menu_File_Export, MenuContext.MainMenu | MenuContext.ToolBarContext));
            file.AddSubMenuItem(MingMenuItemFactory.CreateSeparator(MenuContext.MainMenu));
            file.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(
                MenuFileExit, null, Properties.Resources.Menu_File_Exit, MenuContext.MainMenu));

            items.Add(MingMenuItemFactory.CreateSeparator(MenuContext.TreeViewContext, ContextMenuPosition.AppendToEnd));
            items.Add(MingMenuItemFactory.CreateMenuItem(
                MenuRefresh, Properties.Resources.refresh, Properties.Resources.TreeView_Refresh, MenuContext.TreeViewContext, ContextMenuPosition.AppendToEnd));

            return items;
        }

        public IEnumerable<MingMenuItem> CreateHelpMenu()
        {
            var items = new List<MingMenuItem>();

            var help = MingMenuItemFactory.CreateMenuItem(
                MenuHelp, null, Properties.Resources.Menu_Help, MenuContext.MainMenu);
            items.Add(help);

            help.AddSubMenuItem(MingMenuItemFactory.CreateMenuItem(
                MenuHelpAbout, null, Properties.Resources.Menu_Help_About, MenuContext.MainMenu));

            return items;
        }

        public IEnumerable<string> GetActiveMenuKeys(MingTreeViewItem activeNode)
        {
            var keys = new List<string>();

            keys.Add(MenuFileAddConnection);
            if (activeNode != null)
            {
                keys.Add(MenuFileDeleteConnection);
            }
            keys.Add(MenuFileImport);
            if (SettingsManager.Instance.Connections.Count > 0)
            {
                keys.Add(MenuFileExport);
            }
            keys.Add(MenuFileExit);

            keys.Add(MenuHelpAbout);

            return keys;
        }

        public IEnumerable<string> GetActiveNodeContextMenuKeys(MingTreeViewItem node)
        {
            var items = new List<string>();

            var nodeData = node.Data;

            if (nodeData is TreeViewRootNodeData)
            {
                items.Add(MenuFileDeleteConnection);
            }
            if (node.HasDynamicChildren)
            {
                items.Add(MenuRefresh);
            }

            return items;
        }

        public void MenuItemHandler(ConnectionInfo cnnInfo, MingTreeViewItem node, string menuKey)
        {
            switch (menuKey)
            {
                case MenuFileAddConnection:
                    Connect();
                    break;
                case MenuFileDeleteConnection:
                    Disconnect(cnnInfo);
                    break;
                case MenuFileImport:
                    var import = new ConnectionsPort();
                    import.ImportConnections();
                    break;
                case MenuFileExport:
                    var export = new ConnectionsPort();
                    export.ExportConnections();
                    break;
                case MenuFileExit:
                    End();
                    break;
                case MenuRefresh:
                    _connectionsTreeController.RefreshSelectedNode();
                    break;
                case MenuHelpAbout:
                    var about = new Ming.Forms.AboutForm();
                    about.Owner = _mainWindow;
                    about.ShowDialog();
                    break;
            }
        }

        public ConnectionInfo GetTreeViewNodeConnectionInfo(MingTreeViewItem item)
        {
            if (item == null)
            {
                return null;
            }
            return _connectionsTreeController.GetNodeConnection(item);
        }

        public MingTreeViewItem GetCurrentTreeViewItem()
        {
            return _connectionsTreeController.GetSelectedNode();
        }

        private static volatile MingApp _instance;
        private static object _sync = new Object();

        private MingApp() 
        {
        }

        void ConnectionsTreeController_SelectedItemChanged(MingTreeViewItem selectedItem)
        {
            _menuController.ActivateToolBarItems();
        }

        public static MingApp Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new MingApp();
                        }
                    }
                }

                return _instance;
            }
        }

        public void AddDocumentTab(ITabDocument document, string pluginId)
        {
            _tabDocumentController.AddDocument(document, pluginId);
        }

        public void CloseDocumentTab(int tabId)
        {
            _tabDocumentController.CloseDocumentTab(tabId);
            if (_tabDocumentController.TabCount < 1)
            {
                _menuController.DeactivateAllPluginTabDocumentMenus();
            }
        }

        public void ActiveTabChanged(int activeTabId)
        {
            _activeTabId = activeTabId;
            _menuController.ActivateTabDocumentMenuItems(_tabDocumentController.GetActiveMenuKeysForTab(activeTabId));
        }

        public Window MainWindow
        {
            get { return _mainWindow; }
        }

        public void RefreshTreeViewItem(MingTreeViewItem node)
        {
            _mainWindow.Dispatcher.Invoke(new Action(() => _connectionsTreeController.RefreshNode(node)));
        }

        public void RefreshCurrentTreeViewItem()
        {
            _mainWindow.Dispatcher.Invoke(new Action(() => _connectionsTreeController.RefreshSelectedNode()));
        }

        public void AddLongRunningOperation(OperationStatus status)
        {
            _operations.Add(status);
        }

        public void RemoveLongRunningOperation(OperationStatus status)
        {
            _operations.Remove(status);
        }

        public void CopyRequestedOnTreeviewNode()
        {
            _connectionsTreeController.CopySelectedNode();
        }

        public void SetMenuItemStateForTab(string menuKey, bool enabled, int tabInstanceId)
        {
            _tabDocumentController.SetMenuState(menuKey, enabled, tabInstanceId);
            _menuController.ActivateTabDocumentMenuItems(_tabDocumentController.GetActiveMenuKeysForTab(tabInstanceId));
        }

        public IEnumerable<ConnectionInfo> GetConnectionsForPlugin(string pluginId)
        {
            return SettingsManager.Instance.Connections.Where(cnn => cnn.ServiceId == pluginId).OrderBy(cnn => cnn.Host).ThenBy(cnn => cnn.Port);
        }
    }
}
