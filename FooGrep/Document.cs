using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EncodeDetect;

namespace FooGrep
{
    class Document
    {
        public static Encoding GetCode(string filepath)
        {
            byte[] bytes = new byte[10240];
            FileStream fs = new FileStream(filepath,FileMode.Open,FileAccess.Read);
            fs.Read(bytes,0,bytes.Length);
            return DectingEncode.GetCode(bytes);
        }

        public static IEnumerable<DocumentMatch> Find(string filepath,Encoding enc,string pattern, bool isRegex, RegexOptions opt)
        {
            if (enc == null)
                enc = GetCode(filepath);

            if (enc == null)
                enc = Encoding.Default;

            using(var sr = new StreamReader(filepath, enc))
            {
                if (isRegex == false)
                    pattern = Regex.Escape(pattern);

                Regex ex = new Regex(pattern);

                int lineNumber = 0;
                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();
                    Match m = ex.Match(line);
                    if (m.Success)
                        yield return new DocumentMatch(line, lineNumber);
                    lineNumber++;
                }
            }
        }

        public static void ReplaceAll(string filepath, Encoding enc, string pattern, string replace, bool isRegex, RegexOptions opt, bool isGruop)
        {
            if (enc == null)
                enc = GetCode(filepath);

            var sr = new StreamReader(filepath, enc);

            if (isRegex == false)
                pattern = Regex.Escape(pattern);

            Regex ex = new Regex(pattern);

            string tempPath = filepath + ".tmp";
            StreamWriter sw = new StreamWriter(tempPath,false,enc);

            while (sr.EndOfStream == false)
            {
                string line = sr.ReadLine();
                line = ex.Replace(line, new MatchEvaluator((m) => {
                    if (isGruop)
                        return m.Result(replace);
                    else
                        return replace;
                }));
                sw.WriteLine(line);
            }

            sw.Close();
            sr.Close();

            File.Delete(filepath);
            File.Move(tempPath, filepath);

        }
    }
}
