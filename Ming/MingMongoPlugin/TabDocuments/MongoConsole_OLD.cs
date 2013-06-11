using MingControls.Controls;
using MingPluginInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MingMongoPlugin.TabDocuments
{
    internal class MongoConsole_OLD : ITabDocument
    {
        private readonly EmbeddedConsole _control;
        private int _instanceId;

        public MongoConsole_OLD()
        {
            _control = new EmbeddedConsole();
            _control.StartConsole("C:\\Program Files\\MongoDB\\Mongo.exe", "");
        }

        public System.Windows.Controls.UserControl Control
        {
            get { return _control; }
        }

        public string Title
        {
            get { return "CONSOLE"; /* TODO: RESOURCE */ }
        }

        public string Description
        {
            get { return ""; /* TODO: RESOURCE */ }
        }

        public bool CloseRequested()
        {
            _control.EndProcess();
            return true;
        }

        public int InstanceId
        {
            set { _instanceId = value; }
        }

        public void MenuItemClicked(string menuKey)
        {
            throw new NotImplementedException();
        }
    }
}
