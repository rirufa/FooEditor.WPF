using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using FooEditEngine;
using FooEditEngine.WPF;
using FooEditor;

namespace Outline
{
    static class Actions
    {
        public static void JumpNode(OutlineTreeItem SelectedNode, FooTextBox fooTextBox)
        {
            if (SelectedNode == null)
                return;
            fooTextBox.JumpCaret(SelectedNode.Start);
            fooTextBox.Refresh();
        }

        public static string FitOutlineLevel(string str, OutlineTreeItem item, int childNodeLevel)
        {
            StringReader sr = new StringReader(str);
            StringBuilder text = new StringBuilder();

            string line = sr.ReadLine();
            int level = item.Level;
            int delta = 0;
            if (level > childNodeLevel)
                delta = -1;
            else if (level < childNodeLevel)
                delta = childNodeLevel - level;

            if (delta != 0)
            {
                text.Append(GetNewTitleText(line, level + delta) + "\n");
                while ((line = sr.ReadLine()) != null)
                {
                    level = OutlineAnalyzer.GetWZTextLevel(line);
                    if (level != -1)
                        text.Append(GetNewTitleText(line, level + delta) + "\n");
                    else
                        text.Append(line + "\n");
                }
            }

            sr.Close();
            
            return text.ToString();
        }

        static string GetNewTitleText(string line, int level)
        {
            if (level < 0)
                throw new ArgumentOutOfRangeException();
            StringBuilder output = new StringBuilder();
            for (int i = 0; i <= level; i++)
                output.Append('.');
            output.Append(line.TrimStart(new char[] { '.' }));
            return output.ToString();
        }
    }
}
