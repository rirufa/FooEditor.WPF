using FooEditor;

namespace XmlCompleter
{
    class XmlCompleteItem : CompleteWord
    {
        public bool Attribute
        {
            get;
            set;
        }

        public XmlCompleteItem ParentTag
        {
            get;
            set;
        }

        public XmlCompleteItem(string word, bool attr,XmlCompleteItem parent = null)
            : base(word)
        {
            this.Attribute = attr;
            this.ParentTag = parent;
        }
    }
}
