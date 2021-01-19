using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FooEditEngine;
using FooEditEngine.WPF;
using Prop = FooEditor.Properties;
using FooEditor.Plugin;

namespace FooEditor
{
    /// <summary>
    /// FindReplaceWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class FindReplaceWindow : FindViewBase,IToolWindow,IFindView
    {
        MainWindowViewModel mainvm;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public FindReplaceWindow()
        {
            InitializeComponent();
            this.Title = "検索と置き換え";
            this.FindViewModel.FindHistroy = Config.GetInstance().FindHistroy;
            this.Loaded += FindReplaceWindow_Loaded;
        }

        void FindReplaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.FindParttern.Focus();
        }

        internal FindReplaceWindow(MainWindowViewModel vm) : this()
        {
            vm.Documents.CollectionChanged += Documents_CollectionChanged;
            this.mainvm = vm;
        }

        void Documents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.Reset();
        }

        /// <summary>
        /// ダイアログのタイトルを表す
        /// </summary>
        public string Title
        {
            get;
            set;
        }
        
        /// <summary>
        /// アクティブかどうかを指定する
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(FindReplaceWindow), new PropertyMetadata(false));

        protected override bool ShowFoundPattern
        {
            get
            {
                return Config.GetInstance().ShowFoundPattern;
            }
        }

        protected override string NotFoundInDocumentMessage
        {
            get
            {
                return Prop.Resources.FindDialogNotFound;
            }
        }

        protected override IEnumerable<FooTextBox> GetTextBoxs()
        {
            foreach (DocumentWindow docwnd in this.mainvm.Documents)
            {
                docwnd.Dirty = true;
                yield return docwnd.TextBox;
            }
        }

        protected override IEnumerator<Tuple<FooTextBox, SearchResult>> GetSearchResult(Func<FooTextBox, IEnumerator<SearchResult>> FindStartFunc)
        {
            if (this.FindViewModel.AllDocuments)
            {
                foreach (DocumentWindow docwnd in this.mainvm.Documents)
                {
                    this.mainvm.ActivateDocument(docwnd);

                    IEnumerator<SearchResult> it = FindStartFunc(docwnd.TextBox);
                    while (it.MoveNext())
                    {
                        yield return new Tuple<FooTextBox, SearchResult>(docwnd.TextBox, it.Current);
                    }
                }
            }
            else
            {
                IEnumerator<SearchResult> it = FindStartFunc(this.mainvm.ActiveDocument.TextBox);
                while (it.MoveNext())
                {
                    yield return new Tuple<FooTextBox, SearchResult>(this.mainvm.ActiveDocument.TextBox, it.Current);
                }
            }
        }
    }

}
