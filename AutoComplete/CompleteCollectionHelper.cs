using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using FooEditEngine;
using FooEditor;

namespace AutoComplete
{
    static class CompleteHelper
    {
        /// <summary>
        /// KeywordManager.Operatorsで区切られた単語を補完候補に追加する
        /// </summary>
        /// <param name="s"></param>
        public static void AddCompleteWords(AutocompleteBox box, IList<char> Operators, string s)
        {
            if (box == null || Operators == null)
                return;

            char[] seps = new char[Operators.Count];
            Operators.CopyTo(seps, 0);

            string[] words = s.Split(seps, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
                CompleteHelper.AddComleteWord(box, word);
        }

        /// <summary>
        /// 補完候補を追加する
        /// </summary>
        /// <param name="word"></param>
        public static void AddComleteWord(AutocompleteBox box, string word)
        {
            FooEditor.CompleteWord newItem = new FooEditor.CompleteWord(word);
            if (box.Items.Contains(newItem) == false && CompleteHelper.IsVaildWord(word))
                box.Items.Add(newItem);
        }

        public static string GetWord(Document doc, int startIndex,char[] sep)
        {
            if (doc.Length == 0)
                return null;
            StringBuilder word = new StringBuilder();
            for (int i = startIndex; i >= 0; i--)
            {
                if(sep.Contains(doc[i]))
                {
                    return word.ToString();
                }
                word.Insert(0,doc[i]);
            }
            if (word.Length > 0)
                return word.ToString();
            else
                return null;
        }

        static bool IsVaildWord(string s)
        {
            if (s.Length == 0 || s == string.Empty)
                return false;
            if (!Char.IsLetter(s[0]))
                return false;
            for (int i = 1; i < s.Length; i++)
            {
                if (!Char.IsLetterOrDigit(s[i]))
                    return false;
            }
            return true;
        }
    }
}
