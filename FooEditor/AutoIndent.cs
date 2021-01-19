using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using FooEditEngine;
using FooEditEngine.WPF;

namespace FooEditor
{
    sealed class AutoIndent
    {
        string[] IndentStart, IndentEnd;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="document">対象となるDocumentWindow</param>
        public AutoIndent(DocumentWindow document)
        {
            document.PropertyChanged += new PropertyChangedEventHandler(document_PropertyChanged);
            document.TextBox.AutoIndentHooker = document_AutoIndent;
        }

        /// <summary>
        /// オートインデントを行うなら真。そうでないなら偽
        /// </summary>
        public bool Enable
        {
            get;
            set;
        }

        void document_AutoIndent(object sender, EventArgs e)
        {
            if (this.Enable == false)
                return;

            FooTextBox TextBox = (FooTextBox)sender;

            StringBuilder temp = new StringBuilder();

            TextPoint cur = TextBox.CaretPostion;

            string lineString = TextBox.LayoutLineCollection[cur.row > 0 ? cur.row - 1 : 0];

            int tabNum = this.GetIntendLevel(lineString);

            if (hasWords(lineString, this.IndentEnd))
                tabNum--;
            else if (hasWords(lineString, this.IndentStart))
                tabNum++;

            if (tabNum < 0)
                tabNum = 0;

            for (int i = 0; i < tabNum; i++)
                temp.Append('\t');

            if (temp.Length > 0)
                TextBox.SelectedText = temp.ToString();
        }

        void document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DocumentType")
            {
                DocumentWindow document = (DocumentWindow)sender;
                if (document.SynataxDefnition != null)
                {
                    this.IndentStart = document.SynataxDefnition.IntendStart;
                    this.IndentEnd = document.SynataxDefnition.IntendEnd;
                }
            }
        }

        bool hasWords(string s, IList<string> words)
        {
            if (words == null)
                return false;
            foreach (string word in words)
                if (s.IndexOf(word) != -1)
                    return true;
            return false;
        }

        int GetIntendLevel(string s)
        {
            int level = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != '\t')
                    break;
                level++;
            }
            return level;
        }
    }
}
