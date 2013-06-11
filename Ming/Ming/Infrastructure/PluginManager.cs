using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MingPluginInterfaces;

namespace Ming.Infrastructure
{
    internal class PluginManager
    {
        private List<PluginInfo> _plugins;

        public IList<PluginInfo> Plugins
        {
            get { return _plugins.AsReadOnly(); }
        }

        public IMingPlugin GetPluginInstance(string Id)
        {
            return _plugins.Where(plugin => plugin.Id == Id).First().Instance;
        }

        public PluginInfo GetPluginInfo(string Id)
        {
            return _plugins.Where(plugin => plugin.Id == Id).First();
        }

        private void FindPlugins()
        {
            _plugins = new List<PluginInfo>();

            var path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(PluginManager)).Location);
            var assemblies = Directory.GetFiles(path, "*.dll");

            foreach (var assemblyPath in assemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);

                var implementations = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().ToList().Any(i => i.IsAssignableFrom(typeof(IMingPlugin)))).ToList();

                var idx = 0;
                foreach (var implementation in implementations)
                {
                    var instance = (IMingPlugin)Activator.CreateInstance(implementation);
                    instance.MingApp = MingApp.Instance;
                    var plugin = new PluginInfo(instance.Id, instance.Name, instance);
                    _plugins.Add(plugin);
                    idx++;
                }
            }
        }

        private static volatile PluginManager _instance;
        private static object _sync = new Object();

        private PluginManager() 
        {
            FindPlugins();
        }

        public static PluginManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new PluginManager();
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
