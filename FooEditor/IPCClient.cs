using System;
using System.IO;
using System.IO.Pipes;
using FooEditor.Properties;

namespace FooEditor
{
    sealed class IPCClient
    {
        public IPCClient()
        {
        }

        /// <summary>
        /// パイプサーバーに文字列を送る
        /// </summary>
        /// <param name="pipeName">パイプサーバー名</param>
        /// <param name="str">文字列</param>
        public void Send(string pipeName,string str)
        {
            string result = string.Empty;
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                pipeClient.Connect();
                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    sw.WriteLine(str);
                    sw.Flush();
                }
            }
        }
    }
}
