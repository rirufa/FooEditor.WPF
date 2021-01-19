using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using FooEditEngine;

namespace FooEditor
{
    public sealed class DocumentType : INotifyPropertyChanged
    {
        LineBreakMethod _LineBreakMethod;
        int _LineBreakCharCount, _TabStops;
        string _Extension, _Name;
        bool _IsActive, _IsDrawLineNumber, _IsAutoIndent, _IsDrawRuler, _NoInherit, _ShowFullSpace, _ShowHalfSpace, _ShowTab, _ShowLineBreak, _IndentBySpace;

        /// <summary>
        /// グローバル設定を引きがないなら真を設定する
        /// </summary>
        public bool NoInherit
        {
            get
            {
                return this._NoInherit;
            }
            set
            {
                this._NoInherit = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 対象となる拡張子
        /// </summary>
        public string Extension
        {
            get
            {
                return this._Extension;
            }
            set
            {
                this._Extension = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.OnPropertyChanged();
            }
        }
        public LineBreakMethod LineBreakMethod
        {
            get
            {
                return this._LineBreakMethod;
            }
            set
            {
                this._LineBreakMethod = value;
                this.OnPropertyChanged();
            }
        }
        public int LineBreakCharCount
        {
            get
            {
                return this._LineBreakCharCount;
            }
            set
            {
                this._LineBreakCharCount = value;
                this.OnPropertyChanged();
            }
        }
        public bool IsDrawRuler
        {
            get
            {
                return this._IsDrawRuler;
            }
            set
            {
                this._IsDrawRuler = value;
                this.OnPropertyChanged();
            }
        }
        public bool IsAutoIndent
        {
            get
            {
                return this._IsAutoIndent;
            }
            set
            {
                this._IsAutoIndent = value;
                this.OnPropertyChanged();
            }
        }
        public bool IsDrawLineNumber
        {
            get
            {
                return this._IsDrawLineNumber;
            }
            set
            {
                this._IsDrawLineNumber = value;
                this.OnPropertyChanged();
            }
        }
        public bool IsActive
        {
            get
            {
                return this._IsActive;
            }
            set
            {
                this._IsActive = value;
                this.OnPropertyChanged();
            }
        }
        public bool ShowHalfSpace
        {
            get
            {
                return this._ShowHalfSpace;
            }
            set
            {
                this._ShowHalfSpace = value;
                this.OnPropertyChanged();
            }
        }
        public bool ShowFullSpace
        {
            get
            {
                return this._ShowFullSpace;
            }
            set
            {
                this._ShowFullSpace = value;
                this.OnPropertyChanged();
            }
        }
        public bool ShowTab
        {
            get
            {
                return this._ShowTab;
            }
            set
            {
                this._ShowTab = value;
                this.OnPropertyChanged();
            }
        }
        public bool ShowLineBreak
        {
            get
            {
                return this._ShowLineBreak;
            }
            set
            {
                this._ShowLineBreak = value;
                this.OnPropertyChanged();
            }
        }
        public bool IndentBySpace
        {
            get
            {
                return this._IndentBySpace;
            }
            set
            {
                this._IndentBySpace = value;
                this.OnPropertyChanged();
            }
        }
        public int TabStops
        {
            get
            {
                return this._TabStops;
            }
            set
            {
                this._TabStops = value;
                this.OnPropertyChanged();
            }
        }
        public DocumentType(string name, string ext = null)
        {
            this._Extension = ext;
            this._NoInherit = false;
            this._LineBreakMethod = FooEditEngine.LineBreakMethod.None;
            this._LineBreakCharCount = 80;
            this._IsDrawRuler = false;
            this._IsDrawLineNumber = false;
            this._IsAutoIndent = false;
            this._Name = name;
            this._IsActive = false;
            this._ShowHalfSpace = false;
            this._ShowLineBreak = true;
            this._ShowTab = true;
            this._ShowFullSpace = true;
            this._IndentBySpace = false;
            this._TabStops = 4;
        }

        public DocumentType(string name,string ext, DocumentType type)
        {
            this._Name = name;
            this._Extension = ext;
            this._NoInherit = type._NoInherit;
            this._LineBreakMethod = type._LineBreakMethod;
            this._LineBreakCharCount = type._LineBreakCharCount;
            this._IsDrawRuler = type._IsDrawRuler;
            this._IsDrawLineNumber = type._IsDrawLineNumber;
            this._IsAutoIndent = type._IsAutoIndent;
            this._ShowHalfSpace = type.ShowHalfSpace;
            this._ShowLineBreak = type.ShowLineBreak;
            this._ShowTab = type.ShowTab;
            this._ShowFullSpace = type.ShowFullSpace;
            this._IndentBySpace = type.IndentBySpace;
            this._IsActive = false;
            this._TabStops = type.TabStops;
        }

        public DocumentType(DocumentType type)
            : this(type._Name,type._Extension,type)
        {
        }

        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if(this.PropertyChanged != null)
                this.PropertyChanged(this,new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class DocumentTypeCollection : ObservableCollection<DocumentType>
    {
        public DocumentType SelectedItem
        {
            get;
            private set;
        }

        public new void Add(DocumentType type)
        {
            base.Add(type);
        }

        public void Select(string name)
        {
            this.SelectedItem = null;
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i].Name == name)
                {
                    base[i].IsActive = true;
                    this.SelectedItem = base[i];
                }
                else
                    base[i].IsActive = false;
            }
        }
        public DocumentType Find(string name)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i].Name == name)
                    return base[i];
            }
            return null;
        }
    }
}
