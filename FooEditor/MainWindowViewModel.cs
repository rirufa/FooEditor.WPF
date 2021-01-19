using System;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prop = FooEditor.Properties;
using FooEditEngine;
using FooEditEngine.WPF;
using FooEditor.Plugin;
using System.Runtime.CompilerServices;

namespace FooEditor
{
    internal class MainWindowViewModel : ViewModelBase
    {
        TaskbarManager Taskbar;

        public MainWindowViewModel()
        {
            this.Documents = new ObservableCollection<DocumentWindow>();
            this.Tools = new ObservableCollection<IToolWindow>();

            InputMethod.Current.StateChanged += Current_StateChanged;

            Version ver = Environment.OSVersion.Version;
            if (ver.Major >= 6 && ver.Minor >= 1)
                this.Taskbar = TaskbarManager.Instance;
        }

        private void Current_StateChanged(object sender, InputMethodStateChangedEventArgs e)
        {
            if (e.IsImeConversionModeChanged)
                this.OnPropertyChanged("ImeConversionMode");
            if (e.IsImeStateChanged)
                this.OnPropertyChanged("ImeState");
        }

        public bool DoExepectionProcess(MainWindow main, Exception exception)
        {
            ExceptionDialog dialog = new ExceptionDialog();
            dialog.Exception = exception;
            dialog.Owner = main;
            if (dialog.ShowDialog() == true)
            {
                this.SaveWorkSpacesCommand();
                return false;
            }
            return true;
        }

        /// <summary>
        /// IMEの状態を表す
        /// </summary>
        public InputMethodState ImeState
        {
            get
            {
                return InputMethod.Current.ImeState;
            }
        }

        /// <summary>
        /// IMEの変換モードを表す
        /// </summary>
        public ImeConversionModeValues ImeConversionMode
        {
            get
            {
                return InputMethod.Current.ImeConversionMode;
            }
        }

        DocumentWindow _ActiveDocument;
        public DocumentWindow ActiveDocument
        {
            get
            {
                return this._ActiveDocument;
            }
            private set
            {
                this._ActiveDocument = value;
                this.OnPropertyChanged();
            }
        }

        bool _ProgressNow;
        public bool ProgressNow
        {
            get
            {
                return this._ProgressNow;
            }
            set
            {
                this._ProgressNow = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// ドキュメントコレクション
        /// </summary>
        public ObservableCollection<DocumentWindow> Documents
        {
            get;
            private set;
        }

        /// <summary>
        /// エクスプローラバーコレクション
        /// </summary>
        public ObservableCollection<IToolWindow> Tools
        {
            get;
            private set;
        }

        /// <summary>
        /// 最近履歴
        /// </summary>
        public RecentFileCollection RecentFiles
        {
            get
            {
                Config config = Config.GetInstance();
                return config.RecentFile;
            }
        }

        /// <summary>
        /// 文章モードコレクション
        /// </summary>
        public DocumentTypeCollection DocumentTypes
        {
            get
            {
                Config config = Config.GetInstance();
                return config.SyntaxDefinitions;
            }
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

        /// <summary>
        /// エクスプローラバーを追加する
        /// </summary>
        public void RegisterExploereBar<T>(ExplorerBar<T> bar) where T : IToolWindow
        {
            this.Tools.Add(bar.Content);
        }

        /// <summary>
        /// ドキュメントをアクティブにする
        /// </summary>
        public void ActivateDocument(DocumentWindow doc)
        {
            this.ActiveDocument = doc;
            this.ActiveDocumentChanged(this, null);
        }

        /// <summary>
        /// ドキュメントを作成する
        /// </summary>
        public DocumentWindow CreateDocument()
        {
            DocumentWindow newWindow = new DocumentWindow(this.Documents.Count);
            this.CreatedDocument(this, new DocumentEventArgs(newWindow));
            this.Documents.Add(newWindow);
            newWindow.Progress += Document_Progress;
            return newWindow;
        }

        void Document_Progress(object sender, FooEditEngine.ProgressEventArgs e)
        {
            if (this.Taskbar == null)
                return;
            switch (e.state)
            {
                case FooEditEngine.ProgressState.Start:
                    this.Taskbar.SetProgressState(TaskbarProgressBarState.Indeterminate);
                    this.ProgressNow = true;
                    break;
                case FooEditEngine.ProgressState.Complete:
                    this.Taskbar.SetProgressState(TaskbarProgressBarState.NoProgress);
                    this.ProgressNow = false;
                    break;
            }
        }

        public async Task<bool> ImplMainWindowClosing()
        {
            bool hasDirtyDoc = false;
            foreach (DocumentWindow doc in this.Documents)
                if (doc.Dirty)
                    hasDirtyDoc = true;
            if (hasDirtyDoc == false)
                return false;
#if DEBUG
            //デバック時に「version 6 of comctl32.dll but a different version is current loaded in memory」が表示されるので何もしない
            return false;
#else
            CustomMessageBox dialog = new CustomMessageBox(Prop.Resources.ConfirmDocumentClose, string.Empty);
            dialog.AddButton("save", Prop.Resources.ConfirmDialogSaveButton);
            dialog.AddButton("nosave", Prop.Resources.ConfirmDialogNoSaveButton);
            dialog.AddButton("cancle", Prop.Resources.ConfirmDialogCancelButton);
            string result = dialog.Show();
            if (result == "cancle")
            {
                return true;
            }
            else if (result == "nosave")
            {
                return false;
            }
            foreach (DocumentWindow doc in this.Documents)
            {
                if (doc.Dirty)
                {
                    await this.Save(doc);
                }
            }
            return false;
#endif
        }

        public async Task ImplMainWindowPreviewDrop(string[] filepaths)
        {
            if (filepaths == null)
                return;
            foreach (string filepath in filepaths)
            {
                DocumentWindow document = this.CreateDocument();
                await document.LoadAsync(filepath, null);
            }
        }

        public void ImplMainWindowActived()
        {
            if (this.ActiveDocument != null)
                this.ActiveDocument.TextBox.Focus();
        }

        public void ImpleMainWindowClosed(PluginManager<IPlugin> plugins)
        {
            if (plugins != null)
            {
                foreach (IPlugin plugin in plugins)
                    plugin.ClosedApp();
            }
        }

        public void ImplMainWindowLoad()
        {
            DocumentWindow document = null;
            if (this.LoadWorkSpace())
                document = this.CreateDocument();

            App.ParseArgs(Environment.GetCommandLineArgs());

            if (document != null)
            {
                Task t = document.TextBox.LoadAsync(Console.In, null);
                t.Wait();
            }
        }

        public bool ImplClosingDocument(DocumentWindow document)
        {
            if (document.Dirty == false)
                return false;
#if DEBUG
            //comctl32.dllのバージョンが違いますと表示されるので、メッセージボックスを使う
            MessageBoxResult result = MessageBox.Show(Prop.Resources.ConfirmDocumentClose, document.Title, MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                Task t;
                t = this.Save(document);
                t.Wait();
                return false;
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return true;
            }
#else
            CustomMessageBox dialog = new CustomMessageBox(Prop.Resources.ConfirmDocumentClose, document.Title);
            dialog.AddButton("save", Prop.Resources.ConfirmDialogSaveButton);
            dialog.AddButton("nosave", Prop.Resources.ConfirmDialogNoSaveButton);
            dialog.AddButton("cancle", Prop.Resources.ConfirmDialogCancelButton);
            string result = dialog.Show();
            if (result == "save")
            {
                Task t;
                t = this.Save(document);
                t.Wait();
                return false;
            }
            else if (result == "cancle")
            {
                return true;
            }
#endif
            return false;
        }

        public void ImplCloseDocument(DocumentWindow document)
        {
            this.Documents.Remove(document);
            this.RemovedDocument(this, new DocumentEventArgs(document));
            document.Dispose();

            if (this.Documents.Count == 0)
            {
                this.ActiveDocument = null;
                this.ActiveDocumentChanged(this, null);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void ImplActiveContentChanged(DocumentWindow document)
        {
            if (document == null)
                return;

            this.ActiveDocument = document;
            this.ActiveDocumentChanged(this, null);
        }

        #region Command
        public bool CanExecute()
        {
            bool CanExecute = this.ActiveDocument != null;
            if (this.ActiveDocument != null)
                CanExecute = true;
            else
                CanExecute = false;
            return CanExecute;
        }

        public void GenerateFolding()
        {
            DocumentWindow document = this.ActiveDocument;
            if (document == null)
                return;
            document.TextBox.LayoutLineCollection.GenerateFolding(true);
            document.TextBox.Refresh();
        }

        public void LineJumpCommand(MainWindow main)
        {
            DocumentWindow document = this.ActiveDocument;
            if (document == null)
                return;
            LineJumpDialog dialog = new LineJumpDialog(document.TextBox);
            dialog.Owner = main;
            dialog.ShowDialog();
        }

        public void PrintCommand()
        {
            DocumentWindow document = this.ActiveDocument;
            if (document == null)
                return;
            PrintDialog pd = new PrintDialog();
            pd.PageRangeSelection = PageRangeSelection.AllPages;
            pd.UserPageRangeEnabled = true;
            if (pd.ShowDialog() == false)
                return;
            Config config = Config.GetInstance();
            FooPrintText printtext = new FooPrintText();
            printtext.Document = new Document(document.TextBox.Document);
            printtext.Font = document.TextBox.FontFamily;
            printtext.FontSize = document.TextBox.FontSize;
            printtext.DrawLineNumber = document.TextBox.DrawLineNumber;
            printtext.Header = config.Header;
            printtext.Footer = config.Footer;
            printtext.ParseHF = (s,e)=>{
                PrintInfomation info = new PrintInfomation() { Title = document.Title, PageNumber = e.PageNumber };
                return EditorHelper.ParseHF(e.Original, info);
            };
            printtext.LineBreakMethod = document.TextBox.LineBreakMethod;
            printtext.LineBreakCharCount = document.TextBox.LineBreakCharCount;
            printtext.MarkURL = document.TextBox.MarkURL;
            printtext.Hilighter = document.TextBox.Hilighter;
            printtext.Foreground = document.TextBox.Foreground;
            printtext.URL = document.TextBox.URL;
            printtext.Comment = document.TextBox.Comment;
            printtext.Keyword1 = document.TextBox.Keyword1;
            printtext.Keyword2 = document.TextBox.Keyword2;
            printtext.Litral = document.TextBox.Literal;
            if (pd.PageRangeSelection == PageRangeSelection.AllPages)
            {
                printtext.StartPage = -1;
                printtext.EndPage = -1;
            }
            else
            {
                printtext.StartPage = pd.PageRange.PageFrom;
                printtext.EndPage = pd.PageRange.PageTo;
            }
            printtext.Padding = new Padding((int)config.LeftSpace, (int)config.TopSpace, (int)config.RightSpace, (int)config.BottomSpace);
            printtext.PageRect = new Rect(0,
                0,
                pd.PrintableAreaWidth,
                pd.PrintableAreaHeight);
            printtext.Print(pd);
        }

        public void SelectDocumentTypeCommand(string type)
        {
            DocumentWindow document = this.ActiveDocument;
            if (document == null)
                return;
            document.DocumentType = type;
            this.DocumentTypes.Select(type);
        }

        public void PropertiesCommand(MainWindow main, PluginManager<IPlugin> plugins)
        {
            ConfigDialog cd = new ConfigDialog(plugins);
            cd.Owner = main;
            if (cd.ShowDialog() == false)
                return;
            Config config = Config.GetInstance();
            foreach (DocumentWindow document in this.Documents)
            {
                document.ApplyConfig(config);
            }
        }

        public void GrepCommand()
        {
            Config config = Config.GetInstance();
            if (!File.Exists(config.GrepPath))
                return;
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = config.GrepPath;
            info.Verb = "open";
            info.UseShellExecute = true;
            Process process = Process.Start(info);
        }

        public void NewCommand()
        {
            this.CreateDocument();
        }

        public async Task OpenRecentFileCommand(string filepath)
        {
            DocumentWindow document = this.ActiveDocument;
            if (document == null || document.FilePath != null || document.Dirty)
                document = this.CreateDocument();
            await document.LoadAsync(filepath, null);
        }

        public async Task OpenCommand()
        {
            DocumentWindow document = this.ActiveDocument;
            CustomOpenFileDialog ofd = new CustomOpenFileDialog();
            ofd.addFilter(Prop.Resources.AllFileLable, "*.*");
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (document == null || document.FilePath != null || document.Dirty)
                    document = this.CreateDocument();
                await document.LoadAsync(ofd.FileName, ofd.FileEncoding);
            }
            ofd.Dispose();
        }

        public async Task SaveCommand()
        {
            DocumentWindow document = this.ActiveDocument;
            await this.Save(document);
        }

        public async Task SaveAsCommand()
        {
            DocumentWindow document = this.ActiveDocument;
            await this.SaveAs(document);
        }

        public void HelpCommand()
        {
            Process.Start(Path.Combine(Config.ExecutablePath, Prop.Resources.HelpFileName));
        }

        public void AboutCommand()
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Environment.GetCommandLineArgs()[0]);
            string str = string.Format("{0} version{1}\n{2}", versionInfo.ProductName, versionInfo.ProductVersion, versionInfo.CompanyName);
            MessageBox.Show(str, "FooEditorについて");
#if DEBUG
            throw new NotImplementedException();
#endif
        }
        
        public void SaveWorkSpacesCommand()
        {
            foreach (DocumentWindow document in this.Documents)
            {
                string stateFilePath = string.Format(Config.ApplicationFolder + "\\" + Prop.Resources.RecoveryState, Process.GetCurrentProcess().Id, document.Title);

                if (Directory.Exists(Config.ApplicationFolder) == false)
                    Directory.CreateDirectory(Config.ApplicationFolder);

                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fs = new FileStream(stateFilePath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(fs, document);
                fs.Close();
            }
        }

        #endregion

        bool LoadWorkSpace()
        {
            string recoveryStateFilePattern = string.Format(Prop.Resources.RecoveryState, "*", "");
            if (Directory.Exists(Config.ApplicationFolder) == false)
                return true;
            string[] files = Directory.GetFiles(Config.ApplicationFolder, recoveryStateFilePattern);
            if (files.Length > 0 && MessageBox.Show(Prop.Resources.RecoveryConfirm, "", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                for (int i = 0; i < files.Length; i++)
                    File.Delete(files[i]);
                return true;
            }
            else if (files.Length == 0)
            {
                return true;
            }
            for (int i = 0; i < files.Length; i++)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fs = new FileStream(files[i], FileMode.Open, FileAccess.Read);
                DocumentWindow document = (DocumentWindow)formatter.Deserialize(fs);
                fs.Close();
                File.Delete(files[i]);
                this.CreatedDocument(this, new DocumentEventArgs(document));
                this.Documents.Add(document);
            }
            return false;
        }

        async Task Save(DocumentWindow document)
        {
            if (document == null)
                return;
            if (document.FilePath == string.Empty || document.FilePath == null)
                await this.SaveAs(document);
            else
                await document.SaveAsync(document.FilePath, document.Encoding);
        }

        async Task SaveAs(DocumentWindow document)
        {
            if (document == null)
                return;
            CustomSaveFileDialog sfd = new CustomSaveFileDialog();
            sfd.addFilter(Prop.Resources.AllFileLable, "*.*");
            sfd.FileEncoding = document.Encoding;
            sfd.LineFeed = document.LineFeed;
            if (!String.IsNullOrEmpty(document.FilePath))
                sfd.InitialDirectory = System.IO.Path.GetDirectoryName(document.FilePath);
            if (sfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                await document.SaveAsync(sfd.FileName, sfd.FileEncoding);
                document.FilePath = sfd.FileName;
            }
            sfd.Dispose();
        }

    }
}
sealed class CustomMessageBox
{
    TaskDialog Dialog;
    string ClickedButtonName;
    public CustomMessageBox(string text, string title)
    {
        this.Dialog = new TaskDialog();
        this.Dialog.Caption = title;
        this.Dialog.Text = text;
        this.Dialog.Cancelable = true;
    }
    public void AddButton(string name, String text, bool elevate = false)
    {
        TaskDialogButton button = new TaskDialogButton(name, text);
        button.Click += button_Click;
        button.UseElevationIcon = elevate;
        this.Dialog.Controls.Add(button);
    }

    void button_Click(object sender, EventArgs e)
    {
        TaskDialogButton button = (TaskDialogButton)sender;
        this.ClickedButtonName = button.Name;
        this.Dialog.Close();
    }

    public string Show()
    {
        this.Dialog.Show();
        return this.ClickedButtonName;
    }
}
