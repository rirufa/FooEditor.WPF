//#define TEST_ASYNC

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using EncodeDetect;
using Prop = FooEditor.Properties;
using FooEditEngine;
using FooEditEngine.WPF;
using System.Runtime.CompilerServices;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace FooEditor
{
    /// <summary>
    /// 文章タイプが変化している時に送られてくるイベント
    /// </summary>
    public class DocumentTypeChangeingEventArgs : EventArgs
    {
        /// <summary>
        /// 変更後の文章タイプ
        /// </summary>
        public string Type;
        /// <summary>
        /// イベント処理後にハイライター、フォールディングを割り当てる必要がないなら真を設定する
        /// </summary>
        public bool Handled = false;
        public DocumentTypeChangeingEventArgs(string type)
        {
            this.Type = type;
        }
    }
    /// <summary>
    /// DocumentWindow.xaml の相互作用ロジック
    /// </summary>
    [Serializable]
    public partial class DocumentWindow : UserControl,INotifyPropertyChanged,ISerializable,IDisposable
    {
        int updateCount;
        string _Title, _FilePath, _DocumentType;
        Encoding _Encoding;
        LineFeedType _LineFeed;
        AutoIndent autoIndent;
        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public DocumentWindow()
        {
            InitializeComponent();
            this.PropertyChanged +=new PropertyChangedEventHandler((s,e)=>{});
            this.DocumentTypeChanged += new EventHandler((s, e) => { });
            this.DocumentTypeChangeing += new EventHandler<DocumentTypeChangeingEventArgs>((s, e) => { });
            
            this.Encoding = DectingEncode.GetEncodingFromWebName(Config.GetInstance().DefaultEncoding);
            this.LineFeed = LineFeedType.CRLF;

            this._DocumentType = Prop.Resources.DocumetTypeNone;

            this.autoIndent = new AutoIndent(this);

            Config config = Config.GetInstance();
            this.ApplyConfig(config);
            this.TextBox.Document.Update += new DocumentUpdateEventHandler(Document_Update);
            this.TextBox.MouseDoubleClick += new MouseButtonEventHandler(TextBox_MouseDoubleClick);

            this.ExtraDataCollection = new Dictionary<string, object>();

            this.Progress += (s, e) => { };

        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="number"></param>
        public DocumentWindow(int number) : this()
        {
            this.Title = string.Format(Prop.Resources.NewDocumentTitle,number);
        }

        /// <summary>
        /// 現在開いているドキュメントのパス
        /// </summary>
        public string FilePath
        {
            get { return this._FilePath; }
            set
            {
                this.Title = Path.GetFileName(value);               
                this._FilePath = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// ドキュメントのタイトル
        /// </summary>
        public string Title
        {
            get { return this._Title; }
            set
            {
                this._Title = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 現在開いているファイルのエンコーディング
        /// </summary>
        public Encoding Encoding
        {
            get { return this._Encoding; }
            set
            {
                this._Encoding = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 現在開いているファイルの改行コード
        /// </summary>
        public LineFeedType LineFeed
        {
            get { return this._LineFeed; }
            set
            {
                this._LineFeed = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 現在開いているファイルの文章タイプ
        /// </summary>
        public string DocumentType
        {
            get { return this._DocumentType; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = Prop.Resources.DocumetTypeNone;

                this._DocumentType = value;

                this.ApplyConfig(Config.GetInstance(), value);

                DocumentTypeChangeingEventArgs e = new DocumentTypeChangeingEventArgs(value);
                this.DocumentTypeChangeing(this, e);
                
                if (!e.Handled)
                {
                    if (AttachHeilighter(value))
                        return;
                }

                if (value == Prop.Resources.DocumetTypeNone)
                {
                    this.TextBox.LayoutLineCollection.ClearHilight();
                    this.TextBox.LayoutLineCollection.ClearFolding();
                }
                else
                {
                    this.TextBox.LayoutLineCollection.HilightAll();
                    this.TextBox.LayoutLineCollection.ClearFolding();
                    this.TextBox.LayoutLineCollection.GenerateFolding();
                }
                this.TextBox.Refresh();
                this.OnPropertyChanged();
                this.DocumentTypeChanged(this, null);
            }
        }

        /// <summary>
        /// ドキュメントが変更されたなら真
        /// </summary>
        public bool Dirty
        {
            get { return (bool)GetValue(DirtyProperty); }
            set { SetValue(DirtyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Dirty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DirtyProperty =
            DependencyProperty.Register("Dirty", typeof(bool), typeof(DocumentWindow), new UIPropertyMetadata(false));
        
        /// <summary>
        /// DockPanelを表す
        /// </summary>
        internal DockPanel LayoutRoot
        {
            get { return this.DockPanel; }
        }

        /// <summary>
        /// TextBoxを表す
        /// </summary>
        public FooTextBox TextBox
        {
            get { return this.FooTextBox; }
        }

        /// <summary>
        /// 保持しているキーワードを表す
        /// </summary>
        public SyntaxDefnition SynataxDefnition
        {
            get;
            private set;
        }

        /// <summary>
        /// オートインデントを有効にするなら真
        /// </summary>
        public bool EnableAutoIndent
        {
            get { return this.autoIndent.Enable; }
            set { this.autoIndent.Enable = value; }
        }

        /// <summary>
        /// オートコンプリートボックスへのインスタンスを表す
        /// </summary>
        public AutocompleteBox CompleteBox
        {
            get;
            set;
        }

        /// <summary>
        /// プロパティの変更を通知する
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// DocumentWindowに対応する付属のデーターを表す
        /// </summary>
        public Dictionary<string, object> ExtraDataCollection
        {
            get;
            private set;
        }

        /// <summary>
        /// 文章タイプが変更されたことを通知する
        /// </summary>
        public event EventHandler DocumentTypeChanged;

        /// <summary>
        /// 文章タイプが変更されようとしていることを通知する
        /// </summary>
        public event EventHandler<DocumentTypeChangeingEventArgs> DocumentTypeChangeing;

        /// <summary>
        /// 非同期操作の進行状況を表す
        /// </summary>
        public event EventHandler<ProgressEventArgs> Progress;

        /// <summary>
        /// 設定を適用する
        /// </summary>
        /// <param name="config">Configインスタンス</param>
        public void ApplyConfig(Config config)
        {
            if (!config.ShowFoundPattern)
                this.TextBox.MarkerPatternSet.Remove(FindReplaceWindow.FoundMarkerID);
            this.TextBox.Foreground = config.Fore;
            this.TextBox.Background = config.Back;
            this.TextBox.Comment = config.Comment;
            this.TextBox.ControlChar = config.Control;
            this.TextBox.Keyword1 = config.Keyword1;
            this.TextBox.Keyword2 = config.Keyword2;
            this.TextBox.Literal = config.Literal;
            this.TextBox.Hilight = config.Hilight;
            this.TextBox.URL = config.URL;
            this.TextBox.InsertCaret = config.InsetCaret;
            this.TextBox.OverwriteCaret = config.OverwriteCaret;
            this.TextBox.LineMarker = config.LineMarker;
            this.TextBox.UpdateArea = config.UpdateArea;
            this.TextBox.FontFamily = new FontFamily(config.FontName);
            this.TextBox.FontSize = config.FontSize;
            this.TextBox.DrawCaretLine = config.DrawLine;
            this.TextBox.MarkURL = config.UrlMark;
            this.TextBox.TextAntialiasMode = config.TextAntialiasMode;
            this.TextBox.LineNumber = config.LineNumber;
            this.TextBox.IndentMode = config.IndentBySpace ? IndentMode.Space : IndentMode.Tab;
            this.ApplyConfig(config, this._DocumentType);
            this.TextBox.Refresh();
        }

        void ApplyConfig(Config config, string doctype)
        {
            LineBreakMethod lineBreakMethod;
            int lineBreakCharCount,tabCharCount;
            bool IsAutoIndent, IsDrawRuler, IsDrawLineNumber, ShowFullSpace, ShowHalfSpace, ShowLineBreak, ShowTab, IndentBySpace;
            DocumentType current = doctype == Prop.Resources.DocumetTypeNone ? null : config.SyntaxDefinitions.Find(doctype);
            if (current == null || !current.NoInherit)
            {
                lineBreakMethod = config.LineBreakMethod;
                lineBreakCharCount = config.LineBreakCharCount;
                IsAutoIndent = config.AutoIndent;
                IsDrawRuler = config.DrawRuler;
                IsDrawLineNumber = config.DrawLineNumber;
                ShowFullSpace = config.ShowFullSpace;
                ShowHalfSpace = config.ShowHalfSpace;
                ShowLineBreak = config.ShowLineBreak;
                ShowTab = config.ShowTab;
                IndentBySpace = config.IndentBySpace;
                tabCharCount = config.TabStops;
            }
            else
            {
                lineBreakMethod = current.LineBreakMethod;
                lineBreakCharCount = current.LineBreakCharCount;
                IsAutoIndent = current.IsAutoIndent;
                IsDrawRuler = current.IsDrawRuler;
                IsDrawLineNumber = current.IsDrawLineNumber;
                ShowFullSpace = current.ShowFullSpace;
                ShowHalfSpace = current.ShowHalfSpace;
                ShowLineBreak = current.ShowLineBreak;
                ShowTab = current.ShowTab;
                IndentBySpace = current.IndentBySpace;
                tabCharCount = current.TabStops;
            }
            this.EnableAutoIndent = IsAutoIndent;
            this.TextBox.DrawLineNumber = IsDrawLineNumber;
            this.TextBox.DrawRuler = IsDrawRuler;
            bool rebuild = false;
            this.TextBox.ShowFullSpace = ShowFullSpace;
            this.TextBox.ShowHalfSpace = ShowHalfSpace;
            this.TextBox.ShowLineBreak = ShowLineBreak;
            this.TextBox.IndentMode = IndentBySpace ? IndentMode.Space : IndentMode.Tab;
            this.TextBox.ShowTab = ShowTab;
            this.TextBox.TabChars = tabCharCount;
            if (this.TextBox.LineBreakMethod != lineBreakMethod)
            {
                this.TextBox.LineBreakMethod = lineBreakMethod;
                rebuild = true;
            }
            if (this.TextBox.LineBreakCharCount != lineBreakCharCount)
            {
                this.TextBox.LineBreakCharCount = lineBreakCharCount;
                rebuild = true;
            }
            if (rebuild)
                this.TextBox.PerfomLayouts();
        }

        /// <summary>
        /// ファイルを読み取る
        /// </summary>
        /// <param name="filepath">対象となるファイルへのフルパス</param>
        /// <param name="enc">エンコーディング。nullの場合は自動判定</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task LoadAsync(string filepath, Encoding enc)
        {
            try
            {
                this.Progress(this,new ProgressEventArgs(ProgressState.Start));

                this.TextBox.IsEnabled = false;

                this.TextBox.Refresh();

                if (enc == null)
                {
                    using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                    {
                        enc = await DectingEncode.GetEncodingAsync(fs);
                    }
                    if (enc == null || enc == Encoding.ASCII)
                        enc = DectingEncode.GetEncodingFromWebName(Config.GetInstance().DefaultEncoding);
                    this.Encoding = enc;
                }

                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    this.LineFeed = await LineFeedHelper.GetLineFeedAsync(fs,this.Encoding);
                }

                await this.TextBox.LoadFileAsync(filepath, this.Encoding, this.cancelTokenSource);

                this.DocumentType = this.DeciedKeywordFileName(filepath);

                this.FilePath = filepath;

                Config config = Config.GetInstance();
                
                config.RecentFile.InsertAtFirst(filepath);
                
                config.SyntaxDefinitions.Select(this._DocumentType);

                this.TextBox.JumpCaret(0);

                this.TextBox.IsEnabled = true;

                this.TextBox.Refresh();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Progress(this, new ProgressEventArgs(ProgressState.Complete));
            }
            this.Dirty = false;
            this.updateCount = 0;
        }

        /// <summary>
        /// ファイルを保存する
        /// </summary>
        /// <param name="filepath">対象となるファイル</param>
        /// <param name="enc">文字コードをしていする</param>
        /// <returns>Taskオブジェクト</returns>
        public async Task SaveAsync(string filepath, Encoding enc)
        {
            Config confing = Config.GetInstance();

            if (confing.MaxBackupCount > 0 && File.Exists(filepath))
            {
                string oldFilename = GetBackupFileName(filepath);
                if (oldFilename != null)
                    File.Move(filepath, oldFilename);
            }

            try
            {
                await this.TextBox.SaveFile(filepath, enc, LineFeedHelper.ToString(this.LineFeed),this.cancelTokenSource);
            }
            catch (UnauthorizedAccessException)
            {
                CustomMessageBox dialog = new CustomMessageBox(Prop.Resources.UACSaveConfirm, this.Title);
                dialog.AddButton("save", Prop.Resources.ConfirmDialogSaveButton, true);
                dialog.AddButton("nosave", Prop.Resources.ConfirmDialogNoSaveButton, false);
                string result = dialog.Show();
                if (result == "save")
                {
                    string tempfile = Path.GetTempPath() + Path.GetRandomFileName();
                    await this.TextBox.SaveFile(tempfile, this.Encoding, LineFeedHelper.ToString(this.LineFeed),null);
                    this.DoAdminOperation(tempfile, filepath, false);
                }
            }
            finally
            {
                this.updateCount = 0;
                this.Dirty = false;
            }
        }

        /// <summary>
        /// オブジェクトを破棄する
        /// </summary>
        public void Dispose()
        {
            this.FooTextBox.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void DoAdminOperation(string tempfile, string filename, bool backup)
        {
            AdminiOperation operation = new AdminiOperation();

            if (backup && File.Exists(filename))
            {
                string oldFilename = GetBackupFileName(filename);
                if (oldFilename != null)
                    operation.WriteCode(string.Format("move\t{0}\t{1}", filename, oldFilename));
            }

            operation.WriteCode(string.Format("copy\t{0}\t{1}", tempfile, filename));

            bool result = false;
            try
            {
                result = operation.Execute();
            }
            catch (Win32Exception ex)  //UACのキャンセル時に発生するので
            {
                if (ex.NativeErrorCode != 1223)
                    throw;
            }
            if (result)
            {
                this.FilePath = filename;
                Config cfg = Config.GetInstance();
                cfg.RecentFile.InsertAtFirst(filename);
            }
        }

        void Document_Update(object sender, FooEditEngine.DocumentUpdateEventArgs e)
        {
            Config cfg = Config.GetInstance();
            if (e.type == UpdateType.Replace)
            {
                if (cfg.AutoSaveCount != 0)
                {
                    if (this.updateCount > cfg.AutoSaveCount)
                        this.SaveBackupFolder(this.FilePath);
                    else
                        this.updateCount++;
                }
                if (this.Dirty == false && !this.TextBox.LayoutLineCollection.IsFrozneDirtyFlag)
                    this.Dirty = true;
            }
        }

        async void SaveBackupFolder(string filename)
        {
            if (filename == string.Empty || filename == null)
                filename = Path.Combine(Config.ApplicationFolder, "Backup", this.Title);
            string BackupFilename = filename + ".autosave";
            try
            {
                await this.TextBox.SaveFile(filename,
                    this.Encoding, LineFeedHelper.ToString(this.LineFeed),
                    this.cancelTokenSource);
            }
            catch (UnauthorizedAccessException)
            {
            }
            this.updateCount = 0;
        }

        string GetBackupFileName(string filename)
        {
            string directoryPart = Path.GetDirectoryName(filename);
            string filePart = Path.GetFileName(filename);
            IEnumerable<string> files = Directory.EnumerateFiles(directoryPart, filePart + "*");

            int newCount = files.Count();

            Config cfg = Config.GetInstance();
            if (newCount > cfg.MaxBackupCount)
                return null;

            return filename + "." + newCount;
        }

        private string DeciedKeywordFileName(string EditingFile)
        {
            Config cfg = Config.GetInstance();

            if (EditingFile == null || EditingFile == string.Empty)
                return Prop.Resources.DocumetTypeNone;

            foreach (DocumentType item in cfg.SyntaxDefinitions)
            {
                string type = item.Name;
                string targetExt = item.Extension;
                if (type == Prop.Resources.DocumetTypeNone || targetExt == null || targetExt == string.Empty)
                    continue;
                Regex regex = new Regex(targetExt, RegexOptions.IgnoreCase);
                Match m = regex.Match(Path.GetFileName(EditingFile));
                if (m.Success == true)
                    return type;
            }
            return null;
        }

        private bool AttachHeilighter(string name)
        {
            if (name == null || name == Prop.Resources.DocumetTypeNone || name == string.Empty)
            {
                this.TextBox.Hilighter = null;
                this.TextBox.FoldingStrategy = null;
                return false;
            }

            string filepath = GetKeywordFilePath(name);

            if (filepath == null)
                return true;

            var tuple = EditorHelper.GetFoldingAndHilight(filepath);
            this.TextBox.FoldingStrategy = tuple.Item1;
            this.TextBox.Hilighter = tuple.Item2;

            SyntaxDefnition SynataxDefnition = new SyntaxDefnition();
            SynataxDefnition.generateKeywordList(filepath);
            this.SynataxDefnition = SynataxDefnition;

            return false;
        }

        string GetKeywordFilePath(string name)
        {
            Config cfg = Config.GetInstance();

            string KeywordFolderName = "Keywords";

            string filepath = Path.Combine(Config.ApplicationFolder, KeywordFolderName, name);

            if (File.Exists(filepath))
                return filepath;

            filepath = Path.Combine(Config.ExecutablePath, KeywordFolderName, name);

            if (File.Exists(filepath))
                return filepath;

            return null;
        }

        void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FooMouseButtonEventArgs fe = (FooMouseButtonEventArgs)e;
            foreach (Marker m in this.TextBox.Document.GetMarkers(MarkerIDs.URL,fe.Index))
            {
                if (m.hilight == HilightType.Url)
                {
                    ProcessStartInfo info = new ProcessStartInfo();
                    info.Arguments = "";
                    info.FileName = this.TextBox.Document.ToString(m.start, m.length);
                    info.Verb = "open";
                    info.UseShellExecute = true;
                    Process process = Process.Start(info);

                    fe.Handled = true;
                }
            }
        }

        public DocumentWindow(SerializationInfo info, StreamingContext context) : this()
        {
            this.FilePath = info.GetString("FilePath");
            
            this.Title = info.GetString("Title");
            
            this.DocumentType = info.GetString("DocumentType");
            
            this.Encoding = Encoding.GetEncoding(info.GetInt32("Encoding"));
            
            this.TextBox.InsertMode = info.GetBoolean("InsertMode");
            
            this.TextBox.RectSelectMode = info.GetBoolean("RectSelectMode");

            int maxCount = info.GetInt32("LineCount");
            this.TextBox.Document.FireUpdateEvent = false;
            this.TextBox.Document.UndoManager.BeginLock();
            for (int i = 0; i < maxCount; i++)
                this.TextBox.Document.Append(info.GetString(i.ToString()));
            this.TextBox.Document.UndoManager.EndLock();
            this.TextBox.Document.FireUpdateEvent = true;
            
            TextPoint tp = new TextPoint();
            tp.row = info.GetInt32("row");
            tp.col = info.GetInt32("col");
            this.TextBox.Loaded += new RoutedEventHandler((s,e)=>{
                this.TextBox.JumpCaret(tp.row, tp.col);
                this.TextBox.Refresh();
            });
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("row", this.TextBox.CaretPostion.row);
            
            info.AddValue("col", this.TextBox.CaretPostion.col);
            
            info.AddValue("RectSelectMode", this.TextBox.RectSelectMode);
            
            info.AddValue("Title", this.Title);
            
            info.AddValue("FilePath", this.FilePath);
            
            info.AddValue("DocumentType", this.DocumentType);
            
            info.AddValue("Encoding", this.Encoding.CodePage);
            
            info.AddValue("InsertMode", this.TextBox.InsertMode);
            
            info.AddValue("LineCount", this.TextBox.LayoutLineCollection.Count);
            for (int i = 0; i < this.TextBox.LayoutLineCollection.Count; i++)
                info.AddValue(i.ToString(), this.TextBox.LayoutLineCollection[i]);
        }
    }
}
