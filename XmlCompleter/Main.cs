using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.ComponentModel.Composition;
using FooEditEngine;
using FooEditEngine.WPF;
using FooEditor;
using FooEditor.Plugin;
using XmlCompleter.Properties;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace XmlCompleter
{
    [Export(typeof(IPlugin))]
    public sealed class Main : IPlugin
    {
        MainWindow editor;
        bool inputedTag = false;

        public void Initalize(MainWindow e)
        {
            editor = e;
            editor.CreatedDocument += new EventHandler<DocumentEventArgs>(editor_CratedEditFromEvent);

            MenuItem item = new MenuItem();
            item.Header = Resources.XSDFileMenuItem;
            item.Click += CreateCompleteItemFromXSD;
            MenuItem toolItem = (MenuItem)e.FindName(FooEditorMenuItemName.ToolMenuName);
            toolItem.Items.Insert(0, item);
        }

        private void CreateCompleteItemFromXSD(object sender, EventArgs e)
        {
            DocumentWindow active = this.editor.ActiveDocument;

            if (active == null)
                return;

            CustomOpenFileDialog ofd;
            ofd = new CustomOpenFileDialog();
            ofd.addFilter(Resources.XSDFileFilter, "*.xsd");

            CommonFileDialogResult result = ofd.ShowDialog();
            string filepath = ofd.FileName;

            ofd.Dispose();

            if (result == CommonFileDialogResult.Cancel)
                return;

            AutocompleteBox box = active.CompleteBox;

            XmlTextReader reader = new XmlTextReader(filepath);
            XmlSchema schema = XmlSchema.Read(reader, null);
            if (box != null)
            {
                foreach (XmlSchemaElement element in schema.Items)
                {
                    XmlCompleteItem parent = new XmlCompleteItem(element.Name, false);
                    box.Items.Add(parent);
                    XmlSchemaComplexType type = element.SchemaType as XmlSchemaComplexType;
                    if (type == null)
                        continue;
                    foreach (XmlSchemaAttribute attr in type.Attributes)
                        box.Items.Add(new XmlCompleteItem(attr.Name, true, parent));
                }
            }
            reader.Close();
        }

        void editor_CratedEditFromEvent(object sender, DocumentEventArgs e)
        {
            e.Document.DocumentTypeChanged += new EventHandler(edit_DocumentChangeTypeEvent);
        }

        void edit_DocumentChangeTypeEvent(object sender, EventArgs e)
        {
            DocumentWindow editForm = (DocumentWindow)sender;

            if (editForm.DocumentType == null)
                return;

            if (editForm.SynataxDefnition != null)
            {
                if (editForm.SynataxDefnition.Hilighter == "xml")
                {
                    AutocompleteBox box = new AutocompleteBox(editForm);
                    box.ShowingCompleteBox = new FooEditor.ShowingCompleteBoxEnventHandler(OnPreShow);
                    box.SelectItem = new FooEditor.SelectItemEventHandler(OnDoAutocomplete);
                    box.Items = new FooEditor.CompleteCollection<FooEditor.ICompleteItem>();
                    if(editForm.CompleteBox != null)
                        editForm.CompleteBox.Dispose();
                    editForm.CompleteBox = box;
                }

                AutocompleteBox CompleteBox = editForm.CompleteBox;
                CompleteBox.Items.Clear();

                editForm.CompleteBox.Operators = editForm.SynataxDefnition.Operators;

                foreach (string word in editForm.SynataxDefnition.Keywords)
                    CompleteBox.Items.Add(new XmlCompleteItem(word, false));
                foreach (string word in editForm.SynataxDefnition.Keywords2)
                    CompleteBox.Items.Add(new XmlCompleteItem(word, true));
            }
        }

        private void OnDoAutocomplete(object sender, FooEditor.SelectItemEventArgs e)
        {
            FooTextBox textbox = e.textbox;
            string inputing_word = e.inputing_word;
            string word = e.word;

            //キャレットは入力された文字の後ろにあるので、こうする
            int start = textbox.Selection.Index - inputing_word.Length;
            if (start < 0)
                start = 0;

            textbox.Document.Replace(start, inputing_word.Length, word);

            textbox.Refresh();
        }

        private void OnPreShow(object sender, FooEditor.ShowingCompleteBoxEventArgs e)
        {
            AutocompleteBox box = (AutocompleteBox)sender;

            int inputingIndex = e.textbox.Selection.Index - 1;
            if (inputingIndex < 0)
                inputingIndex = 0;

            string parentTag;            
            this.inputedTag = IsInputedTag(e.textbox.Document, inputingIndex,out parentTag);

            e.inputedWord = GetWord(e.textbox.Document, inputingIndex, box.Operators) + e.KeyChar;

            if (e.inputedWord == null)
                return;

            for (int i = 0; i < box.Items.Count; i++)
            {
                XmlCompleteItem item = (XmlCompleteItem)box.Items[i];
                if (item.word.StartsWith(e.inputedWord))
                {
                    if (inputedTag && !item.Attribute)
                        continue;
                    if (item.ParentTag != null && item.ParentTag.word != parentTag)
                        continue;
                    e.foundIndex = i;
                    break;
                }
            }
        }

        string GetWord(Document doc, int startIndex, char[] sep)
        {
            if (doc.Length == 0)
                return null;
            StringBuilder word = new StringBuilder();
            for (int i = startIndex; i >= 0; i--)
            {
                if (sep.Contains(doc[i]))
                {
                    return word.ToString();
                }
                word.Insert(0, doc[i]);
            }
            if (word.Length > 0)
                return word.ToString();
            else
                return null;
        }

        bool IsInputedTag(Document doc, int index,out string tag)
        {
            bool hasSpace = false;
            StringBuilder word = new StringBuilder();

            if (doc.Length == 0)
            {
                tag = null;
                return false;
            }

            for (int i = index; i >= 0; i--)
            {
                if (doc[i] == ' ')
                {
                    hasSpace = true;
                    word.Clear();
                    continue;
                }
                else if (doc[i] == '<')
                {
                    if (hasSpace)
                    {
                        tag = word.ToString();
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                word.Insert(0,doc[i]);
            }
            tag = null;
            return false;
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
