using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace FooEditor
{
    public sealed class RecentFile
    {
        /// <summary>
        /// ファイル名
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }
        /// <summary>
        /// ファイルパス
        /// </summary>
        public string FilePath
        {
            get;
            private set;
        }
        /// <summary>
        /// コンストラクター
        /// </summary>
        public RecentFile(string name,string path)
        {
            this.FileName = name;
            this.FilePath = path;
        }
    }
    public sealed class RecentFileCollection : ObservableCollection<RecentFile>
    {
        /// <summary>
        /// 最大数
        /// </summary>
        public int MaxCount
        {
            get;
            set;
        }

        /// <summary>
        /// 先頭に挿入する
        /// </summary>
        public void InsertAtFirst(string filepath)
        {
            this.Remove(filepath);
            if (base.Count >= this.MaxCount)
            {
                base.RemoveAt(base.Count - 1);
            }
            base.Insert(0,new RecentFile(TrimFullPath(filepath),filepath));
        }

        public void Add(string filepath)
        {
            if (base.Count >= this.MaxCount)
                return;
            if (this.IndexOf(filepath) != -1)
                return;
            //MenuItemにバインディングする場合、_は__でないといけない
            base.Add(new RecentFile(TrimFullPath(filepath).Replace("_","__"), filepath));
        }

        public void AddRange(IEnumerable<string> collction)
        {
            foreach (string s in collction)
                this.Add(s);
        }

        public void Remove(string filepath)
        {
            int index = this.IndexOf(filepath);
            if (index != -1)
                this.RemoveAt(index);
        }

        public int IndexOf(string filepath)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i].FilePath == filepath)
                    return i;
            }
            return -1;
        }

        public string[] ToArray()
        {
            string[] retval = new string[this.Count];
            for (int i = 0; i < this.Count; i++)
                retval[i] = (string)base[i].FilePath;
            return retval;
        }

        string TrimFullPath(string filepath)
        {
            if (filepath == null || filepath == "")
                return string.Empty;
            string DirectoryPart = Path.GetDirectoryName(filepath);
            string FilenamePart = Path.GetFileName(filepath);
            string[] slice = DirectoryPart.Split('\\');
            if (slice.Length > 3)
            {
                DirectoryPart = slice[0] + "\\..\\" + slice[slice.Length - 1];
                return DirectoryPart + "\\" + FilenamePart;
            }
            else
                return filepath;
        }
    }
}
