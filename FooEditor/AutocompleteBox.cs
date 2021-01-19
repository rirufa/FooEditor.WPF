using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FooEditEngine;
using FooEditEngine.WPF;

namespace FooEditor
{
    public sealed class ShowingCompleteBoxEventArgs : EventArgs
    {
        /// <summary>
        /// 入力された文字
        /// </summary>
        public string KeyChar;
        /// <summary>
        /// 入力した単語と一致したコレクションのインデックス。一致しないなら-1をセットする
        /// </summary>
        public int foundIndex;
        /// <summary>
        /// 入力しようとした単語を設定する
        /// </summary>
        public string inputedWord;
        /// <summary>
        /// 補完対象のテキストボックス
        /// </summary>
        public FooTextBox textbox;
        public ShowingCompleteBoxEventArgs(string keyChar,FooTextBox textbox)
        {
            this.inputedWord = null;
            this.KeyChar = keyChar;
            this.foundIndex = -1;
            this.textbox = textbox;
        }
    }

    public sealed class SelectItemEventArgs : EventArgs
    {
        /// <summary>
        /// 補完候補
        /// </summary>
        public string word;
        /// <summary>
        /// 入力中の単語
        /// </summary>
        public string inputing_word;
        /// <summary>
        /// 補完対象のテキストボックス
        /// </summary>
        public FooTextBox textbox;
        public SelectItemEventArgs(string word, string inputing_word,FooTextBox textbox)
        {
            this.word = word;
            this.inputing_word = inputing_word;
            this.textbox = textbox;
        }
    }

    public delegate void SelectItemEventHandler(object sender,SelectItemEventArgs e);
    public delegate void ShowingCompleteBoxEnventHandler(object sender, ShowingCompleteBoxEventArgs e);

    public class AutocompleteBox : IDisposable
    {
        const int InputLength = 2;  //補完を開始する文字の長さ

        private ListBox listBox1 = new ListBox();
        private Popup popup = new Popup();
        private FooTextBox textBox;
        private string inputedWord;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="document">対象となるDocumentWindow</param>
        public AutocompleteBox(DocumentWindow document)
        {
            document.LayoutRoot.Children.Add(this.popup);
            this.popup.Child = this.listBox1;
            this.listBox1.Height = 200;
            this.listBox1.MouseDoubleClick += listBox1_MouseDoubleClick;
            this.listBox1.KeyDown += listBox1_KeyDown;
            this.popup.Height = 200;
            document.TextBox.PreviewKeyDown += new KeyEventHandler(TextBox_PreviewKeyDown);
            document.TextBox.PreviewTextInput += new TextCompositionEventHandler(TextBox_TextInput);
            this.SelectItem = new SelectItemEventHandler((a, b) => { });
            this.ShowingCompleteBox = new ShowingCompleteBoxEnventHandler((s,e)=>{});
            this.textBox = document.TextBox;
        }

        /// <summary>
        /// 補完すべき単語が選択されたときに発生するイベント
        /// </summary>
        public SelectItemEventHandler SelectItem;
        /// <summary>
        /// UI表示前のイベント
        /// </summary>
        public ShowingCompleteBoxEnventHandler ShowingCompleteBox;

        /// <summary>
        /// 区切り文字のリスト
        /// </summary>
        public char[] Operators
        {
            get;
            set;
        }

        /// <summary>
        /// オートコンプリートの対象となる単語のリスト
        /// </summary>
        public CompleteCollection<ICompleteItem> Items
        {
            get
            {
                return (CompleteCollection<ICompleteItem>)this.listBox1.ItemsSource;
            }
            set
            {
                this.listBox1.ItemsSource = value;
                this.listBox1.DisplayMemberPath = CompleteCollection<ICompleteItem>.ShowMember;
            }
        }

        void TextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            FooTextBox textbox = (FooTextBox)sender;

            if (this.Operators == null ||
                e.Text == "\r" ||
                e.Text == "\n" ||
                this.ShowingCompleteBox == null ||
                (this.popup.IsOpen == false && e.Text == "\b"))
                return;

            ShowingCompleteBoxEventArgs ev = new ShowingCompleteBoxEventArgs(e.Text, textbox);
            ShowingCompleteBox(this, ev);

            if (ev.foundIndex != -1 && ev.inputedWord != null && ev.inputedWord != string.Empty && ev.inputedWord.Length >= InputLength)
            {
                ShowCompleteBox(textbox, ev);
                this.popup.IsOpen = true;
            }
            else
                this.popup.IsOpen = false;
        }

        void ShowCompleteBox(FooTextBox textbox, ShowingCompleteBoxEventArgs ev)
        {
            this.inputedWord = ev.inputedWord;
            DecideListBoxLocation(textbox, this.listBox1);
            this.listBox1.SelectedIndex = ev.foundIndex;
            this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
        }

        void DecideListBoxLocation(FooTextBox textbox,ListBox listbox)
        {
            TextPoint tp = textbox.CaretPostion;
            System.Windows.Point p = textbox.GetPostionFromTextPoint(tp);

            int height = (int)textbox.GetLineHeight(tp.row);

            if (p.Y + listbox.Height + height > textbox.ActualHeight - SystemParameters.HorizontalScrollBarHeight)
                p.Y -= listbox.Height;
            else
                p.Y += height;

            this.popup.PlacementTarget = this.textBox;
            this.popup.Placement = PlacementMode.Relative;
            this.popup.PlacementRectangle = new Rect(p,new Size(listBox1.ActualWidth,listBox1.Height));
        }

        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrl = e.KeyboardDevice.Modifiers == ModifierKeys.Control;
            
            if (this.popup.IsOpen == false)
            {
                if (e.Key == Key.Space && isCtrl)
                {
                    FooTextBox textbox = (FooTextBox)sender;

                    ShowingCompleteBoxEventArgs ev = new ShowingCompleteBoxEventArgs(string.Empty, textbox);
                    ShowingCompleteBox(this, ev);

                    ShowCompleteBox(textbox, ev);
                    this.popup.IsOpen = true;

                    e.Handled = true;
                }
                return;
            }

            switch (e.Key)
            {
                case Key.Escape:
                    this.popup.IsOpen = false;
                    this.textBox.Focus();
                    e.Handled = true;
                    break;
                case Key.Down:
                    if (this.listBox1.SelectedIndex + 1 >= this.listBox1.Items.Count)
                        this.listBox1.SelectedIndex = this.listBox1.Items.Count - 1;
                    else
                        this.listBox1.SelectedIndex++;
                    this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (this.listBox1.SelectedIndex - 1 < 0)
                        this.listBox1.SelectedIndex = 0;
                    else
                        this.listBox1.SelectedIndex--;
                    this.listBox1.ScrollIntoView(this.listBox1.SelectedItem);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    this.popup.IsOpen = false;
                    CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                    this.SelectItem(this, new SelectItemEventArgs(selWord.word, this.inputedWord, this.textBox));
                    e.Handled = true;
                    break;
            }
        }

        void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.popup.IsOpen = false;
            CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
            this.SelectItem(this, new SelectItemEventArgs(selWord.word, this.inputedWord, this.textBox));
        }

        void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.popup.IsOpen = false;
                CompleteWord selWord = (CompleteWord)this.listBox1.SelectedItem;
                this.SelectItem(this, new SelectItemEventArgs(selWord.word, this.inputedWord, this.textBox));
                e.Handled = true;
            }
        }

        public void Dispose()
        {
            this.textBox.PreviewKeyDown -= new KeyEventHandler(TextBox_PreviewKeyDown);
            this.textBox.PreviewTextInput -= new TextCompositionEventHandler(TextBox_TextInput);
        }
    }
}
