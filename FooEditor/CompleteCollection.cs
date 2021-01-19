using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FooEditor
{
    public interface ICompleteItem : INotifyPropertyChanged
    {
        /// <summary>
        /// 補完対象の単語を表す
        /// </summary>
        string word { get; }
    }

    public class CompleteWord : ICompleteItem
    {
        private string _word;
        /// <summary>
        /// コンストラクター
        /// </summary>
        public CompleteWord(string w)
        {
            this._word = w;
            this.PropertyChanged += new PropertyChangedEventHandler((s,e)=>{});
        }

        /// <summary>
        /// 補完候補となる単語を表す
        /// </summary>
        public string word
        {
            get { return this._word; }
            set { this._word = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// プロパティが変更されたことを通知する
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// プロパティが変更されたことを通知する
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public sealed class CompleteCollection<T> : BindingList<T> where T : ICompleteItem
    {
        public const string ShowMember = "word";

        /// <summary>
        /// 補完対象の単語を表す
        /// </summary>
        public CompleteCollection()
        {
            this.LongestItem = default(T);
        }

        /// <summary>
        /// 最も長い単語を表す
        /// </summary>
        public T LongestItem
        {
            get;
            private set;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (T s in collection)
                this.Add(s);
        }

        public new void Add(T s)
        {
            if (this.LongestItem == null)
                this.LongestItem = s;
            if (s.word.Length > this.LongestItem.word.Length)
                this.LongestItem = s;
            base.Add(s);
        }

        public new void Insert(int index, T s)
        {
            if (this.LongestItem == null)
                this.LongestItem = s;
            if (s.word.Length > this.LongestItem.word.Length)
                this.LongestItem = s;
            base.Insert(index, s);
        }

        public new void Clear()
        {
            this.LongestItem = default(T);
            base.Clear();
        }
    }
}
