using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using FooEditor;
using Prop = Snippet.Properties;

namespace Snippet
{
    public class Snippet
    {
        public string Name
        {
            get;
            set;
        }
        public string Data
        {
            get;
            set;
        }
        public Snippet(string name, string data)
        {
            this.Name = name;
            this.Data = data;
        }
    }
    public class SnippetCategory
    {
        public string Name
        {
            get;
            set;
        }
        public string FilePath
        {
            get;
            set;
        }
        public SnippetCategory(string name, string path)
        {
            this.Name = name;
            this.FilePath = path;
        }
    }
    /// <summary>
    /// SnippetWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SnippetWindow : Window
    {
        const string SnippetFolderName = "Sinppets";

        public SnippetWindow()
        {
            InitializeComponent();

            string[] filelist = Directory.GetFiles(Path.Combine(Config.ExecutablePath, SnippetFolderName));
            for (int i = 0; i < filelist.Length; i++)
                this.CategoryList.Items.Add(new SnippetCategory(Path.GetFileName(filelist[i]), filelist[i]));

            if (Directory.Exists(Path.Combine(Config.ApplicationFolder, SnippetFolderName)))
            {
                filelist = Directory.GetFiles(Path.Combine(Config.ApplicationFolder, SnippetFolderName));
                for (int i = 0; i < filelist.Length; i++)
                {
                    string name = Path.GetFileName(filelist[i]);
                    if (this.HasCategory(name) == false)
                        this.CategoryList.Items.Add(new SnippetCategory(name, filelist[i]));
                }
            }

            this.CategoryList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(CategoryList_SelectionChanged);
        }

        public string SelectedText
        {
            get;
            private set;
        }

        bool HasCategory(string name)
        {
            foreach (SnippetCategory category in this.CategoryList.Items)
                if (category.Name == name)
                    return true;
            return false;
        }

        void CategoryList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SnippetCategory category = (SnippetCategory)this.CategoryList.SelectedItem;
            this.SnippetList.Items.Clear();
            foreach (Snippet snippet in this.LoadSnippets(category.FilePath))
                this.SnippetList.Items.Add(snippet);
        }

        IEnumerable<Snippet> LoadSnippets(string path)
        {
            XmlDocument xml = new XmlDocument();

            xml.Load(path);
            XmlNodeList nodes = xml.GetElementsByTagName("sinppet");

            foreach (XmlNode node in nodes)
            {
                string name = null, data = null;
                foreach (XmlNode child in node.ChildNodes)
                {
                    switch (child.LocalName)
                    {
                        case "name":
                            name = child.InnerText;
                            break;
                        case "data":
                            data = Util.Replace(child.InnerText, new string[] { "\t", "\n", "\r", "\\n" }, new string[] { "", "", "", Environment.NewLine });
                            break;
                    }
                }
                if (name == null || data == null)
                    throw new Exception(string.Format(Prop.Resources.IsNotPair, name, data, path));
                yield return new Snippet(name, data);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Snippet snippet = (Snippet)this.SnippetList.SelectedItem;
            this.SelectedText = snippet.Data;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
