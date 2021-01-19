using System;
using System.ComponentModel.Composition;
using System.Text;
using System.IO;
using System.Windows.Controls;
using Microsoft.Win32;
using FooEditEngine.WPF;
using FooEditor;
using FooEditor.Plugin;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AutoComplete
{
    [Export(typeof(IPlugin))]
    sealed class Filter : IPlugin
    {
        MainWindow editor;
        bool CollectDocumentWords;

        public void Initalize(MainWindow e)
        {
            editor = e;
            editor.CreatedDocument += new EventHandler<DocumentEventArgs>(editor_CratedDocumentEvent);
            
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey(Config.RegAppPath + "\\AutoComplete");
            CollectDocumentWords = bool.Parse((string)regkey.GetValue("CollectDocumentWords", "false"));
            regkey.Close();

            MenuItem toolItem = (MenuItem)e.FindName(FooEditorMenuItemName.ToolMenuName);
            MenuItem item = new MenuItem();
            item.Header = "ドキュメントから補完候補を作成する";
            item.Click += CreateCompleteItemFromDoc;
            toolItem.Items.Insert(0,item);
            item = new MenuItem();
            item.Header = "TAGから補完候補を作成する";
            item.Click += CrateCompleteItemFromTAGS;
            toolItem.Items.Insert(0, item);
        }

        public void ClosedApp()
        {
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey(Config.RegAppPath + "\\AutoComplete");
            regkey.SetValue("CollectDocumentWords", CollectDocumentWords);
            regkey.Close();
            return;
        }

        public void ShowConfigForm()
        {
            ConfigWindow config = new ConfigWindow();
            config.Owner = this.editor;
            config.CollectWords = this.CollectDocumentWords;
            if (config.ShowDialog() == true)
                this.CollectDocumentWords = config.CollectWords;
        }

        private void CreateCompleteItemFromDoc(object sender, EventArgs e)
        {
            DocumentWindow document = this.editor.ActiveDocument;
            if (document == null)
                return;

            AutocompleteBox box = (AutocompleteBox)document.CompleteBox;

            if(box != null)
                foreach (string s in document.TextBox.LayoutLineCollection)
                    CompleteHelper.AddCompleteWords(box, document.SynataxDefnition.Operators, s);
        }


        private void CrateCompleteItemFromTAGS(object sender, EventArgs e)
        {
            DocumentWindow document = this.editor.ActiveDocument;
            if (document == null)
                return;

            CustomOpenFileDialog ofd = new CustomOpenFileDialog();

            CommonFileDialogResult result = ofd.ShowDialog();
            string filepath = ofd.FileName;
            Encoding enc = ofd.FileEncoding;

            ofd.Dispose();

            if (result == CommonFileDialogResult.Cancel)
                return;

            AutocompleteBox box = document.CompleteBox;

            StreamReader sr = new StreamReader(filepath, enc);
            while (!sr.EndOfStream)
            {
                string[] tokens = sr.ReadLine().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens[0][0] == '!')
                    continue;
                if(box != null)
                    CompleteHelper.AddComleteWord(box, tokens[0]);
            }
            sr.Close();
        }


        void editor_CratedDocumentEvent(object sender, DocumentEventArgs e)
        {
            e.Document.DocumentTypeChanged += new EventHandler(edit_DocumentChangeTypeEvent);
        }

        void edit_DocumentChangeTypeEvent(object sender, EventArgs e)
        {
            DocumentWindow editForm = (DocumentWindow)sender;

            if (editForm.DocumentType == null)
                return;

            AutocompleteBox box = new AutocompleteBox(editForm);
            box.ShowingCompleteBox = new ShowingCompleteBoxEnventHandler(OnPreShow);
            box.SelectItem = new SelectItemEventHandler(OnDoAutocomplete);
            box.Items = new CompleteCollection<ICompleteItem>();
            editForm.TextBox.TextInput += new System.Windows.Input.TextCompositionEventHandler(TextBox_TextInput);
            if (editForm.CompleteBox != null)
                editForm.CompleteBox.Dispose();
            editForm.CompleteBox = box;

            if (editForm.SynataxDefnition != null)
            {
                AutocompleteBox CompleteBox = editForm.CompleteBox;
                CompleteBox.Items.Clear();
                CompleteBox.Operators = editForm.SynataxDefnition.Operators;

                foreach (string s in editForm.SynataxDefnition.Keywords)
                    CompleteBox.Items.Add(new CompleteWord(s));
                foreach (string s in editForm.SynataxDefnition.Keywords2)
                    CompleteBox.Items.Add(new CompleteWord(s));
            }
        }

        void TextBox_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            AutocompleteBox box = editor.ActiveDocument.CompleteBox;
            FooTextBox textbox = editor.ActiveDocument.TextBox;

            if (this.CollectDocumentWords == false)
                return;
            if (e.Text == "\r")
            {
                int row = textbox.CaretPostion.row - 1;
                if (row < 0)
                    row = 0;
                CompleteHelper.AddCompleteWords(box, editor.ActiveDocument.SynataxDefnition.Operators, textbox.LayoutLineCollection[row]);
            }
        }

        private void OnDoAutocomplete(object sender, SelectItemEventArgs e)
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

        private void OnPreShow(object sender, ShowingCompleteBoxEventArgs e)
        {
            AutocompleteBox box = (AutocompleteBox)sender;

            int inputingIndex = e.textbox.Selection.Index - 1;
            if (inputingIndex < 0)
                inputingIndex = 0;

            e.inputedWord = CompleteHelper.GetWord(e.textbox.Document, inputingIndex, box.Operators) + e.KeyChar;

            if (e.inputedWord == null)
                return;

            for (int i = 0; i < box.Items.Count; i++)
            {
                CompleteWord item = (CompleteWord)box.Items[i];
                if (item.word.StartsWith(e.inputedWord))
                {
                    e.foundIndex = i;
                    break;
                }
            }
        }

    }
}
