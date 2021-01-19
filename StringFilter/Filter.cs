using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls;
using FooEditEngine;
using FooEditEngine.WPF;
using StringFilter.Properties;
using FooEditor;
using FooEditor.Plugin;

namespace StringFilter
{
    static class FilterCommand
    {
        public static RoutedCommand ToUpper = new RoutedCommand("ToUpper", typeof(MainWindow));
        public static RoutedCommand ToLower = new RoutedCommand("ToLower", typeof(MainWindow));
        public static RoutedCommand ToEntity = new RoutedCommand("ToEntity", typeof(MainWindow));
        public static RoutedCommand ToRealChar = new RoutedCommand("ToRealChar", typeof(MainWindow));
        public static RoutedCommand ToHalfWidthChar = new RoutedCommand("ToHalfWidthChar", typeof(MainWindow));
        public static RoutedCommand ToFullWidthChar = new RoutedCommand("ToFullWidthChar", typeof(MainWindow));
        public static RoutedCommand ExpandTab = new RoutedCommand("ExpandTab", typeof(MainWindow));
        public static RoutedCommand ExpandSpace = new RoutedCommand("ExpandSpace", typeof(MainWindow));
        public static RoutedCommand InsertQuotation = new RoutedCommand("InsertQuotation", typeof(MainWindow));
    }
    [Export(typeof(IPlugin))]
    sealed class Filter : IPlugin
    {
        MainWindow editor;
        #region IPlugin メンバー

        public void Initalize(MainWindow e)
        {
            this.editor = e;

            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ToUpper, this.ToUpper,this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ToLower, this.ToLower, this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ToEntity, this.ToEntity, this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ToRealChar, this.ToRealChar, this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ToHalfWidthChar, this.ToHalfWidthChar, this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ToFullWidthChar, this.ToFullWidthChar, this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ExpandTab, this.ExpandTab, this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.ExpandSpace, this.ExpandSpace, this.CanExecute));
            this.editor.CommandBindings.Add(new CommandBinding(FilterCommand.InsertQuotation, this.InsertQuotation, this.CanExecute));

            MenuItem rootItem = new MenuItem();
            rootItem.Header = Resource.StringFilterMenuItem;

            MenuItem item = new MenuItem();
            item.Header = Resource.ToUpperMenuItem;
            item.Command = FilterCommand.ToUpper;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToLowerMenuItem;
            item.Command = FilterCommand.ToLower;
            rootItem.Items.Add(item);
            
            item = new MenuItem();
            item.Header = Resource.ToEntityMenuItem;
            item.Command = FilterCommand.ToEntity;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToRealCharMenuItem;
            item.Command = FilterCommand.ToRealChar;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToHalfWidthChar;
            item.Command = FilterCommand.ToHalfWidthChar;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToFullWidthChar;
            item.Command = FilterCommand.ToFullWidthChar;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ExpandTabMenuItem;
            item.Command = FilterCommand.ExpandTab;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ExpandSpaceMenuItem;
            item.Command = FilterCommand.ExpandSpace;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.InsertQuotationMenuItem;
            item.Command = FilterCommand.InsertQuotation;
            rootItem.Items.Add(item);

            MenuItem editMenuItem = (MenuItem)this.editor.FindName(FooEditorMenuItemName.EditMenuName);
            editMenuItem.Items.Add(new Separator());
            editMenuItem.Items.Add(rootItem);

            //同じインスタンスを使いまわすことはできない
            rootItem = new MenuItem();
            rootItem.Header = Resource.StringFilterMenuItem;

            item = new MenuItem();
            item.Header = Resource.ToUpperMenuItem;
            item.Command = FilterCommand.ToUpper;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToLowerMenuItem;
            item.Command = FilterCommand.ToLower;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToEntityMenuItem;
            item.Command = FilterCommand.ToEntity;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToRealCharMenuItem;
            item.Command = FilterCommand.ToRealChar;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToHalfWidthChar;
            item.Command = FilterCommand.ToHalfWidthChar;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ToFullWidthChar;
            item.Command = FilterCommand.ToFullWidthChar;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ExpandTabMenuItem;
            item.Command = FilterCommand.ExpandTab;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.ExpandSpaceMenuItem;
            item.Command = FilterCommand.ExpandSpace;
            rootItem.Items.Add(item);

            item = new MenuItem();
            item.Header = Resource.InsertQuotationMenuItem;
            item.Command = FilterCommand.InsertQuotation;
            rootItem.Items.Add(item);

            ContextMenu contextMenu = (ContextMenu)editor.FindResource(FooEditorResourceName.ContextMenuName);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(rootItem);
        }

        public void ClosedApp()
        {
        }

        public void ShowConfigForm()
        {
        }
        #endregion

        void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DocumentWindow document = this.editor.ActiveDocument;
            if (document == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        void InsertQuotation(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;
            
            if (TextBox.RectSelectMode)
                return;
            
            TextBox.SelectedText = InsertLineHead(TextBox.SelectedText, ">");
            TextBox.Refresh();
        }

        void ToUpper(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;
            
            TextBox.SelectedText = TextBox.SelectedText.ToUpper();
            TextBox.Refresh();
        }

        void ToLower(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;
            
            TextBox.SelectedText = TextBox.SelectedText.ToLower();
            TextBox.Refresh();
        }

        void ToEntity(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;

            string[] oldVales = new string[] { "\"", "&", "<", ">" };
            string[] newVales = new string[] {"&quot;","&amp;","&lt;","&gt;" };

            TextBox.SelectedText = Replace(TextBox.SelectedText,oldVales,newVales);
            TextBox.Refresh();
        }

        void ToRealChar(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;

            string[] oldVales = new string[] { "&quot;", "&amp;", "&lt;", "&gt;" };
            string[] newVales = new string[] { "\"", "&", "<", ">" };

            TextBox.SelectedText = Replace(TextBox.SelectedText, oldVales, newVales);
            TextBox.Refresh();
        }

        void ToHalfWidthChar(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;

            string[] oldVales = new string[] { "１", "２", "３", "４", "５", "６", "７", "８", "９", "０",
                "！", "”", "＃", "＄", "％", "＆", "’", "（", "）", "－", "＾", "￥", "＠", "；", "：",
                "、", "。", "・", "「", "」", "＝", "～", "｜", "‘", "｛", "＋", "＊", "｝", "＜", "＞",
                "？", "＿", "Ａ", "Ｂ", "Ｃ", "Ｄ", "Ｅ", "Ｆ", "Ｇ", "Ｈ", "Ｉ", "Ｊ", "Ｋ", "Ｌ", "Ｍ", 
                "Ｎ", "Ｏ", "Ｐ", "Ｑ", "Ｒ", "Ｓ", "Ｔ", "Ｕ", "Ｖ", "Ｗ", "Ｘ", "Ｙ", "Ｚ", "ａ", "ｂ", 
                "ｃ", "ｄ", "ｅ", "ｆ", "ｇ", "ｈ", "ｉ", "ｊ", "ｋ", "ｌ", "ｍ", "ｎ", "ｏ", "ｐ", "ｑ", 
                "ｒ", "ｓ", "ｔ", "ｕ", "ｖ", "ｗ", "ｘ", "ｙ", "ｚ"
            };
            string[] newVales = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
                "!", "\"", "#", "$", "%", "&", "'", "(", ")", "-", "^", "\\", "@", ";", ":",
                ",", ".", "/", "[", "]", "=", "~", "|", "`", "{", "+", "*", "}", "<", ">",
                "?", "_", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
                "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", 
                "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", 
                "r", "s", "t", "u", "v", "w", "x", "y", "z"
            };

            TextBox.SelectedText = Replace(TextBox.SelectedText, oldVales, newVales);
            TextBox.Refresh();
        }

        void ToFullWidthChar(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;

            string[] oldVales = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
                "!", "\"", "#", "$", "%", "&", "'", "(", ")", "-", "^", "\\", "@", ";", ":",
                ",", ".", "/", "[", "]", "=", "~", "|", "`", "{", "+", "*", "}", "<", ">",
                "?", "_", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
                "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", 
                "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", 
                "r", "s", "t", "u", "v", "w", "x", "y", "z"
            };
            string[] newVales = new string[] { "１", "２", "３", "４", "５", "６", "７", "８", "９", "０",
                "！", "”", "＃", "＄", "％", "＆", "’", "（", "）", "－", "＾", "￥", "＠", "；", "：",
                "、", "。", "・", "「", "」", "＝", "～", "｜", "‘", "｛", "＋", "＊", "｝", "＜", "＞",
                "？", "＿", "Ａ", "Ｂ", "Ｃ", "Ｄ", "Ｅ", "Ｆ", "Ｇ", "Ｈ", "Ｉ", "Ｊ", "Ｋ", "Ｌ", "Ｍ", 
                "Ｎ", "Ｏ", "Ｐ", "Ｑ", "Ｒ", "Ｓ", "Ｔ", "Ｕ", "Ｖ", "Ｗ", "Ｘ", "Ｙ", "Ｚ", "ａ", "ｂ", 
                "ｃ", "ｄ", "ｅ", "ｆ", "ｇ", "ｈ", "ｉ", "ｊ", "ｋ", "ｌ", "ｍ", "ｎ", "ｏ", "ｐ", "ｑ", 
                "ｒ", "ｓ", "ｔ", "ｕ", "ｖ", "ｗ", "ｘ", "ｙ", "ｚ"
            };

            TextBox.SelectedText = Replace(TextBox.SelectedText, oldVales, newVales);
            TextBox.Refresh();
        }

        void ExpandTab(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;

            int TabStops = TextBox.TabChars;
            StringBuilder temp = new StringBuilder();
            int index = 0;
            string Text = TextBox.SelectedText;
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '\t')
                {
                    int tablen = TabStops - (index % TabStops);
                    for (int j = tablen; j > 0; j--)
                        temp.Append(' ');
                    index += tablen;
                }
                else
                {
                    temp.Append(Text[i]);
                    index++;
                }
                if (Text[i] == '\n')
                    index = 0;
            }
            TextBox.SelectedText = temp.ToString();
            TextBox.Refresh();
        }

        void ExpandSpace(object sender, EventArgs e)
        {
            FooTextBox TextBox = this.editor.ActiveDocument.TextBox;

            if (TextBox.RectSelectMode)
                return;

            Regex regex = new Regex(string.Format(" {{1,{0}}}", TextBox.TabChars));
            TextBox.SelectedText = regex.Replace(TextBox.SelectedText,"\t");
            TextBox.Refresh();
        }


        string InsertLineHead(string s, string str)
        {
            string[] lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
                output.AppendLine(str + lines[i]);
            return output.ToString();
        }

        string RemoveLineHead(string s, string str)
        {
            string[] lines = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].StartsWith(str))
                    output.AppendLine(lines[i].Substring(1));
                else
                    output.AppendLine(lines[i]);
            return output.ToString();
        }

        string Replace(string s, string[] oldValues, string[] newValues)
        {
            if (oldValues.Length != newValues.Length)
                throw new ArgumentException("oldValuesとnewValuesの数が一致しません");

            StringBuilder str = new StringBuilder(s);
            for (int i = 0; i < oldValues.Length; i++)
                str = str.Replace(oldValues[i], newValues[i]);
            return str.ToString();
        }
    }
}
