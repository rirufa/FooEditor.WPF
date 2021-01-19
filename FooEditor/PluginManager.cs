using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace FooEditor
{
    public sealed class PluginManager<T> : IEnumerable<T>
    {
        [ImportMany]
        IEnumerable<Lazy<T>> plugins = null;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public PluginManager(List<string> DontLoadList)
        {
            string GlobalPluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugin");

            var catalog = new AggregateCatalog();
            List<string> loadedList = new List<string>();
            AppendCatalogFromDirByAssembly(loadedList, catalog, Config.ApplicationFolder, "*.dll",DontLoadList);
            AppendCatalogFromDirByAssembly(loadedList, catalog, GlobalPluginPath, "*.dll",DontLoadList);
            CompositionContainer container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        /// <summary>
        /// イテレーターを取得する
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (Lazy<T> plugin in this.plugins)
                yield return plugin.Value;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        void AppendCatalogFromDirByAssembly(List<string> loadedAssemblies, AggregateCatalog catalog, string dir, string pattern, List<string> DontLoadList)
        {
            if (Directory.Exists(dir) == false)
                return;
            foreach (string path in System.IO.Directory.GetFiles(dir, pattern))
            {
                string fileName = Path.GetFileName(path);
                if (!loadedAssemblies.Contains(fileName) &&
                    !DontLoadList.Contains(fileName))
                {
                    var ac = new AssemblyCatalog(path);
                    catalog.Catalogs.Add(ac);
                    loadedAssemblies.Add(fileName);
                }
            }
        }
    }
}
