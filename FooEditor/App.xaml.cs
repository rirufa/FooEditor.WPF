using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Runtime;
using Prop = FooEditor.Properties;

namespace FooEditor
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        static Mutex mutex = new Mutex(false, Prop.Resources.ProcessMutex);

        protected override void OnStartup(StartupEventArgs e)
        {
            if (mutex.WaitOne(0, false) == false)
            {
                ParseArgs(Environment.GetCommandLineArgs());
                mutex.Close();
                mutex = null;
                this.Shutdown();
                return;
            }
            if (Directory.Exists(Config.ApplicationFolder) == false)
                Directory.CreateDirectory(Config.ApplicationFolder);
            ProfileOptimization.SetProfileRoot(Config.ApplicationFolder);
            ProfileOptimization.StartProfile("FooEditorProfile");
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if(mutex != null)
                mutex.ReleaseMutex();
            base.OnExit(e);
        }

        public static void ParseArgs(string[] args)
        {
            IPCClient client = new IPCClient();
            if (args.Length == 1)
            {
                client.Send(FooEditor.MainWindow.PipeServerName, "ACTIVE");
                return;
            }
            for (int i = 1; i < args.Length; )
            {
                switch (args[i].ToLower())
                {
                    case "-new":
                        client.Send(FooEditor.MainWindow.PipeServerName, "NEWDOC");
                        i++;
                        break;
                    case "-linejump":
                        if (i + 1 <= args.Length - 1)
                        {
                            client.Send(FooEditor.MainWindow.PipeServerName, string.Format("LINEJUMP\t{0}", args[i + 1]));
                            i += 2;
                        }
                        break;
                    case "-openwithlinejump":
                        if (i + 2 <= args.Length - 1)
                        {
                            client.Send(FooEditor.MainWindow.PipeServerName, string.Format("OPENWITHLINEJUMP\t{0}\t{1}", args[i + 1], args[i + 2]));
                            i += 3;
                        }
                        break;
                    case "-open":
                        if (i + 1 <= args.Length - 1)
                        {
                            client.Send(FooEditor.MainWindow.PipeServerName, string.Format("OPEN\t{0}", args[i + 1]));
                            i += 2;
                        }
                        break;
                    default:
                        i++;
                        break;
                }
            }
        }
    }
}
