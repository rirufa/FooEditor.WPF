using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using FooEditor;
using FooEditor.Plugin;
using System.Windows;
using System.Windows.Input;
using Outline.Properties;
using FooEditEngine;
using FooEditEngine.WPF;

namespace Outline
{
    [Export(typeof(IPlugin))]
    public sealed class Outlline : IPlugin
    {
        MainWindow editor;
        ExplorerBar<OutlineWindow> outlineWindow;

        public void Initalize(MainWindow e)
        {
            this.editor = e;
            this.editor.ActiveDocumentChanged += new EventHandler(editor_ActiveDocumentChanged);
            this.editor.CreatedDocument += editor_CreatedDocument;
            this.editor.RemovedDocument += editor_RemovedDocument;

            this.outlineWindow = new ExplorerBar<OutlineWindow>();
            this.outlineWindow.Content = new OutlineWindow();
            this.editor.RegisterExploereBar<OutlineWindow>(this.outlineWindow);
            this.outlineWindow.IsVisible = false;

            MenuItem item = new MenuItem();
            item.Header = Resources.OutLineMenuName;
            item.Click += new RoutedEventHandler(item_Click);
            MenuItem toolMenuItem = (MenuItem)this.editor.FindName(FooEditorMenuItemName.LookMenuName);
            toolMenuItem.Items.Insert(0, item);

            Config config = Config.GetInstance();
            config.SyntaxDefinitions.Add(new DocumentType(Resources.DocumentTypeWZText));
        }

        void editor_RemovedDocument(object sender, DocumentEventArgs e)
        {
            e.Document.DocumentTypeChangeing -= this.Document_DocumentTypeChangeing;
            e.Document.DocumentTypeChanged -= Document_DocumentTypeChanged;
        }

        void editor_CreatedDocument(object sender, DocumentEventArgs e)
        {
            e.Document.DocumentTypeChangeing += Document_DocumentTypeChangeing;
            e.Document.DocumentTypeChanged += Document_DocumentTypeChanged;
        }

        void Document_DocumentTypeChanged(object sender, EventArgs e)
        {
            this.outlineWindow.Content.ReGenerate();
        }

        void Document_DocumentTypeChangeing(object sender, DocumentTypeChangeingEventArgs e)
        {
            DocumentWindow doc = (DocumentWindow)sender;
            if (e.Type == Resources.DocumentTypeWZText)
            {
                doc.TextBox.Hilighter = null;
                doc.TextBox.FoldingStrategy = new WZTextFoldingGenerator();
                e.Handled = true;
            }
        }

        void editor_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (this.editor.ActiveDocument == null)
                return;
            this.outlineWindow.Content.Target = this.editor.ActiveDocument;
            this.outlineWindow.Content.ReGenerate();
        }

        void item_Click(object sender, RoutedEventArgs e)
        {
            if (this.editor.ActiveDocument == null)
                return;
            this.outlineWindow.Content.Target = this.editor.ActiveDocument;
            this.outlineWindow.IsVisible = true;
            this.outlineWindow.Content.ReGenerate();
        }

        public void ClosedApp()
        {
        }

        public void ShowConfigForm()
        {
        }
    }
}
