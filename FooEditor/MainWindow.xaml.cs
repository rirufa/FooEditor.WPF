//#define TEST_ASYNC
using System;
using Xceed.Wpf.AvalonDock;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Prop = FooEditor.Properties;
using FooEditor.Plugin;
using System.Windows.Media;

namespace FooEditor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        IPCServer Server;
        ExplorerBar<FindReplaceWindow> findWindow;
        PluginManager<IPlugin> plugins;
        MainWindowViewModel vm = new MainWindowViewModel();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.vm.CreatedDocument += new EventHandler<DocumentEventArgs>((s, e) => {
                if(this.CreatedDocument != null)
                    this.CreatedDocument(this, e);
            });
            this.vm.RemovedDocument += new EventHandler<DocumentEventArgs>((s, e) => {
                if(this.RemovedDocument != null)
                    this.RemovedDocument(this, e);
            });
            this.vm.ActiveDocumentChanged += new EventHandler((s, e) => {
                if(this.DockManager.ActiveContent != this.vm.ActiveDocument)
                    this.DockManager.ActiveContent = this.vm.ActiveDocument;
                if(this.ActiveDocumentChanged != null)
                    this.ActiveDocumentChanged(this, e);
            });

            this.DataContext = this.vm;

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewCommand));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAsCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, PrintCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, FindCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Replace, FindCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties, PropertiesCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, HelpCommand));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.SelectDocumentType, SelectDocumentTypeCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.OpenRecentFile, OpenRecentFileCommand));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.Grep, GrepCommand));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.SaveWorkSpace, SaveWorkSpcae, CanExecute));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.Quit, QuitCommand));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.About, AboutCommand));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.LineJump, LineJumpCommand, CanExecute));
            this.CommandBindings.Add(new CommandBinding(FooEditorCommands.GenerateFolding, GenerateFolding, CanExecute));

            this.DockManager.ActiveContentChanged += new EventHandler(DockManager_ActiveContentChanged);
            this.DockManager.DocumentClosed += new EventHandler<DocumentClosedEventArgs>(DockManager_DocumentClosed);
            this.DockManager.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(DockManager_DocumentClosing);
            this.DockManager.MouseDoubleClick += (s, e) => {
                if(this.ActiveDocument != null)
                {
                    FrameworkElement f = this.DockManager.ActiveContent as FrameworkElement;
                    Point p = e.GetPosition(f);
                    //エクスプーラーバー内、ドキュメント内でクリックした場合はnullにはならない
                    if (VisualTreeHelper.HitTest(f, p) != null)
                        return;
                }
                this.vm.NewCommand();
            };
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            this.Closing += new CancelEventHandler(MainWindow_Closing);
            this.Closed += new EventHandler(MainWindow_Closed);
            this.Activated += new EventHandler(MainWindow_Activated);
            this.PreviewDragOver += new DragEventHandler(MainWindow_PreviewDragOver);
            this.PreviewDrop += new DragEventHandler(MainWindow_PreviewDrop);

            Config config = Config.GetInstance();
            this.Width = config.Width;
            this.Height = config.Height;
            this.plugins = new PluginManager<IPlugin>(config.DontLoadPlugins);
            foreach (IPlugin plugin in this.plugins)
                plugin.Initalize(this);

            this.findWindow = new ExplorerBar<FindReplaceWindow>();
            this.findWindow.Content = new FindReplaceWindow(this.vm);
            this.RegisterExploereBar(this.findWindow);
            this.findWindow.IsVisible = false;

            //Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = this.vm.DoExepectionProcess(this, e.Exception);
        }

        /// <summary>
        /// アクティブドキュメントが変更されたことを通知する
        /// </summary>
        public event EventHandler ActiveDocumentChanged;

        /// <summary>
        /// ドキュメントが作成されたことを通知する
        /// </summary>
        public event EventHandler<DocumentEventArgs> CreatedDocument;

        /// <summary>
        /// ドキュメントが削除されたことを通知する
        /// </summary>
        public event EventHandler<DocumentEventArgs> RemovedDocument;

        public DocumentWindow ActiveDocument
        {
            get
            {
                return this.vm.ActiveDocument;
            }
        }

        public ObservableCollection<DocumentWindow> Documents
        {
            get
            {
                return this.vm.Documents;
            }
        }

        /// <summary>
        /// パイプサーバー名
        /// </summary>
        public static string PipeServerName
        {
            get
            {
                return Prop.Resources.IPCServerName + "." + Process.GetCurrentProcess().SessionId;
            }
        }

        /// <summary>
        /// エクスプローラバーを追加する
        /// </summary>
        public void RegisterExploereBar<T>(ExplorerBar<T> bar) where T : IToolWindow
        {
            this.vm.RegisterExploereBar(bar);
        }

        /// <summary>
        /// ドキュメントをアクティブにする
        /// </summary>
        public void ActivateDocument(DocumentWindow doc)
        {
            this.vm.ActivateDocument(doc);
        }

        /// <summary>
        /// ドキュメントを作成する
        /// </summary>
        public DocumentWindow CreateDocument()
        {
            return this.vm.CreateDocument();
        }

        void MainWindow_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        async void MainWindow_PreviewDrop(object sender, DragEventArgs e)
        {
            string[] filepaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            await this.vm.ImplMainWindowPreviewDrop(filepaths);
        }

        async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = await this.vm.ImplMainWindowClosing();
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            this.vm.ImplMainWindowActived();
        }

        void MainWindow_Closed(object sender, System.EventArgs e)
        {
            this.vm.ImpleMainWindowClosed(this.plugins);
            Config config = Config.GetInstance();
            config.Width = this.Width;
            config.Height = this.Height;
            config.Save();
            if (this.Server != null)
                this.Server.Dispose();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Server = new IPCServer(MainWindow.PipeServerName);
            Server.Recive += new ServerReciveEventHandler(Server_Recive);

            this.vm.ImplMainWindowLoad();
        }

        void Server_Recive(object sender, ServerReciveEventArgs e)
        {
            string data = e.StreamReader.ReadLine();

            PipeCommandListener listener = new PipeCommandListener(this);

            listener.Execute(data);
        }

        void DockManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            DocumentWindow document = (DocumentWindow)e.Document.Content;
            e.Cancel = this.vm.ImplClosingDocument(document);
        }

        void DockManager_DocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            DocumentWindow document = (DocumentWindow)e.Document.Content;
            this.vm.ImplCloseDocument(document);
        }

        void DockManager_ActiveContentChanged(object sender, EventArgs e)
        {
            DocumentWindow document = this.DockManager.ActiveContent as DocumentWindow;
            this.vm.ImplActiveContentChanged(document);
        }

        #region Command
        void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.vm.CanExecute();
        }

        void GenerateFolding(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.GenerateFolding();
        }

        void LineJumpCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.LineJumpCommand(this);
        }

        void PrintCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.PrintCommand();
        }

        void SelectDocumentTypeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.SelectDocumentTypeCommand((string)e.Parameter);
        }

        void PropertiesCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.PropertiesCommand(this, this.plugins);
            System.Windows.Media.Color color = Config.GetInstance().FoundMarker;
            this.findWindow.Content.FoundMarkerColor = new FooEditEngine.Color(color.A, color.R, color.G, color.B);
        }

        void QuitCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        void GrepCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.GrepCommand();
        }

        void NewCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.NewCommand();
        }

        void SaveWorkSpcae(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.SaveWorkSpacesCommand();
        }

        async void OpenRecentFileCommand(object sender, ExecutedRoutedEventArgs e)
        {
            await this.vm.OpenRecentFileCommand((string)e.Parameter);
        }

        async void OpenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            await this.vm.OpenCommand();
        }

        async void SaveCommand(object sender, ExecutedRoutedEventArgs e)
        {
            await this.vm.SaveCommand();
        }
        
        async void SaveAsCommand(object sender, ExecutedRoutedEventArgs e)
        {
            await this.vm.SaveAsCommand();
        }

        void FindCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.findWindow.IsVisible = true;
        }

        void HelpCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.HelpCommand();
        }

        void AboutCommand(object sender, ExecutedRoutedEventArgs e)
        {
            this.vm.AboutCommand();
        }
        #endregion

    }

    public static class FooEditorMenuItemName
    {
        public static string FileMenuName = "FileMenuItem";
        public static string EditMenuName = "EditMenuItem";
        public static string LookMenuName = "LookMenuItem";
        public static string ToolMenuName = "ToolMenuItem";
    }

    public static class FooEditorResourceName
    {
        public static string ContextMenuName = "ContextMenu";
    }

    public sealed class DocumentEventArgs : EventArgs
    {
        public DocumentWindow Document;
        public DocumentEventArgs(DocumentWindow doc)
        {
            this.Document = doc;
        }
    }
}
