using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snippet
{
    sealed class Util
    {
        public static string Replace(string s, string[] oldValues, string[] newValues)
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
