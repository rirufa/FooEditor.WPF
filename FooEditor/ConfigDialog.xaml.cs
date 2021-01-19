using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FooEditEngine;
using FooEditor.Plugin;
using System.Reflection;
using Prop = FooEditor.Properties;

namespace FooEditor
{
    public class PluginInfomation
    {
        public string Name
        {
            get;
            private set;
        }
        
        public string Version
        {
            get;
            private set;
        }
        
        public string FileName
        {
            get;
            private set;
        }
        
        public IPlugin Plugin
        {
            get;
            private set;
        }

        public PluginInfomation(string name, string filename)
        {
            this.Name = name;
            this.FileName = filename;
            this.Version = string.Empty;
        }
        
        public PluginInfomation(IPlugin plugin)
        {
            AssemblyName asm = plugin.GetType().Assembly.GetName();
            this.Name = asm.Name;
            this.Version = asm.Version.ToString();
            this.FileName = Path.GetFileName(asm.CodeBase);
            this.Plugin = plugin;
        }
    }
    /// <summary>
    /// ConfigDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigDialog : Window
    {
        List<string> DontLoadPlugins = new List<string>();

        public ConfigDialog()
        {
            InitializeComponent();
        }
        public ConfigDialog(PluginManager<IPlugin> plugins)
            :this()
        {
            this.DataContext = this;

            Config config = Config.GetInstance();

            foreach (IPlugin plugin in plugins)
            {
                this.PluginList.Items.Add(new PluginInfomation(plugin));
            }

            DocumentType docTypeNone = config.SyntaxDefinitions.Find(Prop.Resources.DocumetTypeNone);
            this.DocumentTypeCollection = new DocumentTypeCollection();
            foreach (DocumentType doctype in config.SyntaxDefinitions)
                if(doctype.Extension != null)
                    this.DocumentTypeCollection.Add(new DocumentType(doctype));

            this.LineBreakMethodCollecion = new ObservableCollection<LineBreakMethod>();
            this.LineBreakMethodCollecion.Add(LineBreakMethod.None);
            this.LineBreakMethodCollecion.Add(LineBreakMethod.CharUnit);
            this.LineBreakMethodCollecion.Add(LineBreakMethod.PageBound);

            foreach (var str in config.DontLoadPlugins)
            {
                this.DontLoadPlugins.Add(str);
                string pluginName = Path.GetFileNameWithoutExtension(str);
                if (this.IsLoadedPlugin(pluginName) == false)
                {
                    PluginInfomation pluginInfo = new PluginInfomation(pluginName, str);
                    this.PluginList.Items.Add(pluginInfo);
                }
            }

            RegionInfo info = new RegionInfo(CultureInfo.CurrentUICulture.LCID);
            this.IsMetric = info.IsMetric;
        }

        public List<string> FontCollection
        {
            get
            {
                return FontEnumrator.GetFonts();
            }
        }

        public DocumentTypeCollection DocumentTypeCollection
        {
            get;
            private set;
        }

        public ObservableCollection<LineBreakMethod> LineBreakMethodCollecion
        {
            get;
            private set;
        }

        public Config Config
        {
            get
            {
                return Config.GetInstance();
            }
        }

        public bool IsMetric
        {
            get;
            set;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.BindingGroup.UpdateSources();

            if (this.BindingGroup.HasValidationError)
                return;

            Config config = Config.GetInstance();

            config.SyntaxDefinitions.Clear();
            foreach (DocumentType doctype in this.DocumentTypeCollection)
                config.SyntaxDefinitions.Add(new DocumentType(doctype));

            config.DontLoadPlugins.Clear();
            foreach (string fileName in this.DontLoadPlugins)
            {
                config.DontLoadPlugins.Add(fileName);
            }

            this.DialogResult = true;
            this.Close();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddSyntax_Click(object sender, RoutedEventArgs e)
        {
            DocumentType label = new DocumentType("NewItem");
            this.DocumentTypeCollection.Add(label);
            this.SyntaxNameList.SelectedItem = label;
            this.SyntaxNameList.ScrollIntoView(label);
        }

        private void RemoveSyntax_Click(object sender, RoutedEventArgs e)
        {
            int i;
            for (i = 0; i < this.DocumentTypeCollection.Count; i++)
            {
                DocumentType item = this.DocumentTypeCollection[i];
                if (item.Name == this.SyntaxFileName.Text)
                    break;
            }
            if (i < this.DocumentTypeCollection.Count)
                this.DocumentTypeCollection.RemoveAt(i);
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            PluginInfomation info = (PluginInfomation)this.PluginList.SelectedItem;
            if(info.Plugin != null)
                info.Plugin.ShowConfigForm();
        }

        private void TogglePlugin_Click(object sender, RoutedEventArgs e)
        {
            PluginInfomation info = (PluginInfomation)this.PluginList.SelectedItem;
            if (info == null)
                return;
            if (info.Version != string.Empty)
                this.DontLoadPlugins.Add(info.FileName);
            else
                this.DontLoadPlugins.Remove(info.FileName);
        }

        private bool IsLoadedPlugin(string name)
        {
            foreach (PluginInfomation info in this.PluginList.Items)
            {
                if (info.Name == name)
                    return true;
            }
            return false;
        }
    }
}
