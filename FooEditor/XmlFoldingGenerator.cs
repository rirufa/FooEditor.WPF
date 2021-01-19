using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using FooEditEngine;

namespace FooEditor
{
    class XmlFoldingGenerator : IFoldingStrategy
    {
        public IEnumerable<FoldingItem> AnalyzeDocument(Document doc, int start, int end)
        {
            DocumentReader docReader = new DocumentReader(doc);
            XmlReader xmlReader = XmlReader.Create(docReader);
            Stack<int> beginIndexs = new Stack<int>();
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        beginIndexs.Push(0);
                        break;
                    case XmlNodeType.EndElement:
                        if (beginIndexs.Count == 0)
                            continue;
                        int beginIndex = beginIndexs.Pop();
                        //yield return new FoldingItem(beginIndex,0);
                        break;
                }
            }
            xmlReader.Close();
            docReader.Close();
            throw new NotImplementedException();
        }
    }
}
