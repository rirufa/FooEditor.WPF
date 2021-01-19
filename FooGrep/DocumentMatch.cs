using System;
using System.Text.RegularExpressions;

namespace FooGrep
{
    struct DocumentMatch
    {
        public string foundString;
        public int lineNumber;

        public DocumentMatch(string line,int lineNumber)
        {
            this.foundString = line;
            this.lineNumber = lineNumber;
        }
    }
}
