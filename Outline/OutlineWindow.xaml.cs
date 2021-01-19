using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FooEditor;
using Microsoft.Win32;
using FooEditEngine;
using FooEditor.Plugin;
using FooEditEngine.WPF;

namespace Outline
{
    public static class OutlineCommands
    {
        public static RoutedCommand PasteAsChild = new RoutedCommand("PasteAsChild", typeof(OutlineWindow));
        public static RoutedCommand UpLevel = new RoutedCommand("UpLevel", typeof(OutlineWindow));
        public static RoutedCommand DownLevel = new RoutedCommand("DownLevel", typeof(OutlineWindow));
        public static RoutedCommand Copy = new RoutedCommand("Copy", typeof(OutlineWindow));
        public static RoutedCommand Cut = new RoutedCommand("Cut", typeof(OutlineWindow));
    }
    public class AnalyzePattern
    {
        public string Type;
        public string[] Patterns;
        public override string ToString()
        {
            return this.Type;
        }
        public AnalyzePattern(string type, string[] patterns)
        {
            this.Type = type;
            this.Patterns = patterns;
        }
    }
    /// <summary>
    /// OutlineWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class OutlineWindow : UserControl,IToolWindow
    {
        const string OutlineAnalyzePatternPath = Config.RegAppPath + "\\OutlineAnalyzePattern";
        DocumentWindow _Target;

        public OutlineWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.realTimeAnalyze = true;

            this.CommandBindings.Add(new CommandBinding(OutlineCommands.PasteAsChild, PasteAsChildCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(OutlineCommands.UpLevel, UpLevelCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(OutlineCommands.DownLevel, DownLevelCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(OutlineCommands.Copy, CopyCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(OutlineCommands.Cut, CutCommand, CanExecute));
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(OutlineWindow), new PropertyMetadata(false));

        public string Title
        {
            get { return Properties.Resources.OutLineMenuName; }
        }

        public DocumentWindow Target
        {
            get { return this._Target; }
            set
            {
                this._Target = value;
                this._Target.TextBox.Document.Update += new FooEditEngine.DocumentUpdateEventHandler(Document_Update);
            }
        }

        public bool realTimeAnalyze
        {
            get;
            set;
        }

        public void ReGenerate()
        {
            FooTextBox textbox = this.Target.TextBox;
            OutlineAnalyzer.Analyze(this.TreeView, textbox.FoldingStrategy, textbox.LayoutLineCollection,textbox.Document);
        }
        
        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                OutlineTreeItem item = (OutlineTreeItem)this.TreeView.SelectedItem;
                Actions.JumpNode(item, this.Target.TextBox);
            }
        }

        void Document_Update(object sender, DocumentUpdateEventArgs e)
        {
            if (this.realTimeAnalyze == false)
                return;
            FooTextBox textbox = this.Target.TextBox;
            OutlineAnalyzer.Analyze(this.TreeView, textbox.FoldingStrategy, textbox.LayoutLineCollection, textbox.Document);
        }

        #region Command
        void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            FooTextBox textbox = this.Target.TextBox;
            e.CanExecute = textbox.FoldingStrategy is WZTextFoldingGenerator;
        }

        void CutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OutlineWindow window = (OutlineWindow)sender;
            OutlineTreeItem treeitem = window.TreeView.SelectedItem as OutlineTreeItem;
            if (treeitem == null)
                return;

            this.SetToClipboard(treeitem);
            this.Target.TextBox.Document.Remove(treeitem.Start, treeitem.End - treeitem.Start + 1);
            this.Target.TextBox.Refresh();
        }

        void CopyCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OutlineWindow window = (OutlineWindow)sender;
            OutlineTreeItem treeitem = window.TreeView.SelectedItem as OutlineTreeItem;
            if (treeitem == null)
                return;

            this.SetToClipboard(treeitem);
        }

        void SetToClipboard(OutlineTreeItem treeitem)
        {
            FooTextBox textbox = this.Target.TextBox;
            string text = textbox.Document.ToString(treeitem.Start, treeitem.End - treeitem.Start + 1);
            Clipboard.SetText(text);
        }

        void PasteAsChildCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OutlineWindow window = (OutlineWindow)sender;
            OutlineTreeItem treeitem = window.TreeView.SelectedItem as OutlineTreeItem;
            if (treeitem == null)
                return;

            FooTextBox textbox = this.Target.TextBox;

            string text = Actions.FitOutlineLevel(Clipboard.GetText(), treeitem, treeitem.Level + 1);

            textbox.Document.Replace(treeitem.End + 1, 0, text);
            textbox.Refresh();
        }

        void UpLevelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OutlineWindow window = (OutlineWindow)sender;
            OutlineTreeItem item = window.TreeView.SelectedItem as OutlineTreeItem;
            if (item == null)
                return;

            FooTextBox textbox = this.Target.TextBox;
            string text = textbox.Document.ToString(item.Start, item.End - item.Start + 1);
            text = Actions.FitOutlineLevel(text, item, item.Level + 1);

            textbox.Document.Replace(item.Start, item.End - item.Start + 1, text);
            textbox.Refresh();

        }

        void DownLevelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            OutlineWindow window = (OutlineWindow)sender;
            OutlineTreeItem item = window.TreeView.SelectedItem as OutlineTreeItem;

            if (item == null || item.Level == 0)
                return;

            FooTextBox textbox = this.Target.TextBox;

            string text = textbox.Document.ToString(item.Start, item.End - item.Start + 1);
            text = Actions.FitOutlineLevel(text, item, item.Level - 1);

            textbox.Document.Replace(item.Start, item.End - item.Start + 1, text);

            textbox.Refresh();
        }
        #endregion
    }
}
