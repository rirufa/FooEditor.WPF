using System;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using FooEditor.Properties;

namespace FooEditor
{
    sealed class ServerReciveEventArgs : EventArgs
    {
        /// <summary>
        /// クライアントからのリクエストを格納するオブジェクト
        /// </summary>
        /// <remarks>サーバー側で閉じられるので、使用後にClose()を呼ぶ必要はありません</remarks>
        public StreamReader StreamReader;
        public ServerReciveEventArgs(StreamReader sr)
        {
            this.StreamReader = sr;
        }
    }

    /// <summary>
    /// クライアントから受信時に呼び出されるイベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    delegate void ServerReciveEventHandler(object sender,ServerReciveEventArgs e);

    sealed class IPCServer : IDisposable
    {
        private EventWaitHandle closeApplicationEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private string ServerName;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="serverName">サーバー名</param>
        public IPCServer(string serverName)
        {
            this.ServerName = serverName;
            this.Recive += new ServerReciveEventHandler((s, e) => { });
            Thread Thread = new Thread(pipeServerThread);
            Thread.Start();
        }

        /// <summary>
        /// クライアントから何か送られてきたときに実行されるイベント
        /// </summary>
        /// <remarks>
        /// このイベントはオブジェクトを生成したのとは別のスレッドで実行されます。
        /// </remarks>
        public event ServerReciveEventHandler Recive;

        #region IDisposable メンバー

        public void Dispose()
        {
            closeApplicationEvent.Set();
            closeApplicationEvent.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
        void pipeServerThread(object o)
        {
            NamedPipeServerStream pipeServer = null;
            try
            {
                while (true)
                {
                    pipeServer = new NamedPipeServerStream(this.ServerName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    IAsyncResult async = pipeServer.BeginWaitForConnection(null, null);
                    int index = WaitHandle.WaitAny(new WaitHandle[] { async.AsyncWaitHandle, closeApplicationEvent });
                    switch (index)
                    {
                        case 0:
                            pipeServer.EndWaitForConnection(async);
                            using (StreamReader sr = new StreamReader(pipeServer))
                            {
                                this.Recive(this, new ServerReciveEventArgs(sr));
                            }
                            if (pipeServer.IsConnected)
                                pipeServer.Disconnect();
                            break;
                        case 1:
                            return;
                    }
                }
            }
            finally
            {
                if (pipeServer != null)
                    pipeServer.Close();
            }
        }

    }
}
