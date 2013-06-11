using MingPluginInterfaces;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ming.Infrastructure
{
    internal class TabDocument
    {
        private static int _idCounter = 0;

        private readonly ITabDocument _document;
        private readonly int _id;

        private readonly List<string> _activeMenuKeys;

        private readonly string _pluginId;

        public TabDocument(ITabDocument document, string pluginId)
        {
            _document = document;
            _idCounter++;
            _id = _idCounter;
            _pluginId = pluginId;
            _activeMenuKeys = new List<string>();
        }

        public string Title { get { return _document.Title; } }

        public string Description { get { return _document.Description; } }

        public UserControl Control { get { return _document.Control; } }

        public int Id { get { return _id; } }

        public ITabDocument ITabDocumentInterface { get { return _document; } }

        public string PluginId { get { return _pluginId; } }

        public void AddActiveMenu(string menuKey)
        {
            if (!_activeMenuKeys.Contains(menuKey))
            {
                _activeMenuKeys.Add(menuKey);
            }
        }

        public void RemoveActiveMenu(string menuKey)
        {
            if (_activeMenuKeys.Contains(menuKey))
            {
                _activeMenuKeys.Remove(menuKey);
            }
        }

        public IEnumerable<string> GetActiveMenuKeys()
        {
            return _activeMenuKeys.AsReadOnly();
        }
    }
}
