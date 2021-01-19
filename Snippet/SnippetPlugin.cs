using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using FooEditEngine;
using FooEditEngine.WPF;
using FooEditor;
using FooEditor.Plugin;
using Snippet.Properties;

namespace Snippet
{
    static class SnippetCommand
    {
        public static RoutedCommand InsertSnippet = new RoutedCommand("InsertSnipper", typeof(MainWindow));
    }
    [Export(typeof(IPlugin))]
    sealed class SnippetPlugin : IPlugin
    {
        MainWindow editor;

        public void Initalize(MainWindow e)
        {
            editor = e;

            editor.CommandBindings.Add(new CommandBinding(SnippetCommand.InsertSnippet,this.item_Click,this.CanExecute));

            MenuItem item = new MenuItem();
            item.Header = Resources.MenuItemName;
            item.Command = SnippetCommand.InsertSnippet;
            MenuItem editMenuItem = (MenuItem)editor.FindName(FooEditorMenuItemName.EditMenuName);
            editMenuItem.Items.Add(new Separator());
            editMenuItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resources.MenuItemName;
            item.Command = SnippetCommand.InsertSnippet;
            ContextMenu contextMenu = (ContextMenu)editor.FindResource(FooEditorResourceName.ContextMenuName);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(item);
        }

        void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DocumentWindow document = this.editor.ActiveDocument;
            if (document == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        void item_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SnippetWindow dlg = new SnippetWindow();
            dlg.Owner = this.editor;
            dlg.ShowDialog();
            if (dlg.SelectedText != null)
            {
                FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

                int lineNumber = TextBox.CaretPostion.row;
                string lineString = TextBox.LayoutLineCollection[lineNumber];
                int tabNum = lineString.Count((c) => { return c == '\t'; });

                StringBuilder tabs = new StringBuilder();
                for (int i = 0; i < tabNum; i++)
                    tabs.Append("\t");
                string[] oldValues = new string[] { "${encode}", "\\n", "\\t", "\\i" };
                string[] newValues = new string[] { this.editor.ActiveDocument.Encoding.WebName, System.Environment.NewLine, "\t", tabs.ToString() };

                TextBox.SelectedText = Util.Replace(dlg.SelectedText, oldValues, newValues);
                TextBox.Refresh();
            }
        }

        public void ClosedApp()
        {
            return;
        }

        public void ShowConfigForm()
        {
            return;
        }
    }
}
