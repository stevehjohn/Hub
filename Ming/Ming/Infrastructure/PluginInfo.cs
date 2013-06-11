using System;
using System.Linq;
using MingPluginInterfaces;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ming.Infrastructure
{
    internal class PluginInfo
    {
        private readonly string _id;
        private readonly string _name;
        private readonly IMingPlugin _instance;
        private readonly IEnumerable<MingMenuItem> _menuItems;

        public string Id { get { return _id; } }
        public string Name { get { return _name; } }
        public IMingPlugin Instance { get { return _instance; } }

        public PluginInfo(string id, string name, IMingPlugin instance)
        {
            _id = id;
            _name = name;
            _instance = instance;

            _menuItems = _instance.AllMenuItems;
        }
    }
}
