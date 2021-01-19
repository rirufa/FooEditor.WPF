using System;
using System.IO;
using System.Windows.Forms;

namespace AdminOperation
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// 
        /// 使用する際はコマンドを各行に書き込み、EOFを最終行に書き込んでください
        /// 区切り文字はタブを使用してください
        /// 
        /// 以下のコマンドが使用できます
        /// copy file1 file2
        /// 　file1をfile2にコピーする
        /// move file1 file2
        /// 　file1をfile2に移動する
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length != 1)
                return 1;

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(args[0]);
                string line;
                do
                {
                    line = sr.ReadLine();
                    if (line[0] != '\u001a')
                    {
                        string[] param = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        IOperation operation = OperationFactory.Create(param[0]);
                        operation.Execute(param);
                    }
                } while (line[0] != '\u001a');
            }
#if !DEBUG
            catch(Exception ex){
                MessageBox.Show(ex.Message);
                return 1;
            }
#endif
            finally
            {
                if(sr != null)
                    sr.Close();
            }

            return 0;
        }

    }
}
