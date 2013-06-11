using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Ming.Infrastructure;

namespace Ming.Controllers
{
    internal class TabDocumentController
    {
        private readonly TabControl _container;
        private ObservableCollection<TabDocument> _documents;

        public TabDocumentController(TabControl container)
        {
            _container = container;
            _documents = new ObservableCollection<TabDocument>();
            _container.ItemsSource = _documents;
        }

        public void AddDocument(ITabDocument document, string pluginId)
        {
            var newTab = new TabDocument(document, pluginId);
            _documents.Add(newTab);
            document.InstanceId = newTab.Id; 
            _container.SelectedIndex = _documents.Count - 1;
        }

        public void CloseDocumentTab(int tabId)
        {
            var tab = _documents.Where(doc => doc.Id == tabId).First();
            if (tab.ITabDocumentInterface.CloseRequested())
            {
                _documents.Remove(tab);
            }
        }

        public void SetMenuState(string menuKey, bool isActive, int tabInstanceId)
        {
            var inst = _documents.First(tab => tab.Id == tabInstanceId);
            if (inst != null)
            {
                if (isActive)
                {
                    inst.AddActiveMenu(menuKey);
                }
                else
                {
                    inst.RemoveActiveMenu(menuKey);
                }
            }
        }

        public string GetPluginIdForTab(int tabDocumentId)
        {
            return _documents.First(doc => doc.Id == tabDocumentId).PluginId;
        }

        public IEnumerable<string> GetActiveMenuKeysForTab(int tabDocumentId)
        {
            return _documents.First(doc => doc.Id == tabDocumentId).GetActiveMenuKeys();
        }

        public int TabCount
        {
            get
            {
                return _documents.Count;
            }
        }

        public TabDocument GetTabDocument(int tabDocumentId)
        {
            return _documents.First(tab => tab.Id == tabDocumentId);
        }
    }
}
