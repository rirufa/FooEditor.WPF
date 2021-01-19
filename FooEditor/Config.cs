using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using FooEditEngine;
using FooEditEngine.WPF;
using FooEditor.Properties;

namespace FooEditor
{
    public sealed class Config : DependencyObject
    {
        static private Config _instance;

        public const string RegAppPath = "Software\\FooProject\\FooEditor";
        const string SyntaxDefinitionPath = RegAppPath + "\\SytaxDefinition";
        const string ConfigVersion = "2";

        static void ValidateUnSignedIntegerNumber(DependencyObject d,DependencyPropertyChangedEventArgs e)
        {
            int n = (int)e.NewValue;
            if (n < 0)
                throw new ArithmeticException(Properties.Resources.VaildateErrorOnUnsignedNumber);
        }

        static void ValidateUnSignedDoubleNumber(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            double n = (double)e.NewValue;
            if (n < 0)
                throw new ArithmeticException(Properties.Resources.VaildateErrorOnUnsignedNumber);
        }

        ObservableCollection<string> _history = new ObservableCollection<string>();
        public ObservableCollection<string> FindHistroy
        {
            get
            {
                return this._history;
            }
        }

        public string DefaultEncoding
        {
            get { return (string)GetValue(DefaultEncodingProperty); }
            set { SetValue(DefaultEncodingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultEncoding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultEncodingProperty =
            DependencyProperty.Register("DefaultEncoding", typeof(string), typeof(Config), new PropertyMetadata(null));



        public bool ShowFoundPattern
        {
            get { return (bool)GetValue(ShowFoundPatternProperty); }
            set { SetValue(ShowFoundPatternProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowFoundPattern.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowFoundPatternProperty =
            DependencyProperty.Register("ShowFoundPattern", typeof(bool), typeof(Config), new PropertyMetadata(false));

        /// <summary>
        /// 半角スペースを表示するかどうか
        /// </summary>
        public bool ShowHalfSpace
        {
            get { return (bool)GetValue(ShowHalfSpaceProperty); }
            set { SetValue(ShowHalfSpaceProperty, value); }
        }

        public static readonly DependencyProperty ShowHalfSpaceProperty =
            DependencyProperty.Register("ShowHalfSpace", typeof(bool), typeof(Config), new PropertyMetadata(false));

        /// <summary>
        /// 全角スペースを表示するかどうか
        /// </summary>
        public bool ShowFullSpace
        {
            get { return (bool)GetValue(ShowFullSpaceProperty); }
            set { SetValue(ShowFullSpaceProperty, value); }
        }

        public static readonly DependencyProperty ShowFullSpaceProperty =
            DependencyProperty.Register("ShowFullSpace", typeof(bool), typeof(Config), new PropertyMetadata(true));

        /// <summary>
        /// タブを表示するかどうか
        /// </summary>
        public bool ShowTab
        {
            get { return (bool)GetValue(ShowTabProperty); }
            set { SetValue(ShowTabProperty, value); }
        }

        public static readonly DependencyProperty ShowTabProperty =
            DependencyProperty.Register("ShowTab", typeof(bool), typeof(Config), new PropertyMetadata(true));

        /// <summary>
        /// 改行を表示するかどうか
        /// </summary>
        public bool ShowLineBreak
        {
            get { return (bool)GetValue(ShowLineBreakProperty); }
            set { SetValue(ShowLineBreakProperty, value); }
        }

        public static readonly DependencyProperty ShowLineBreakProperty =
            DependencyProperty.Register("ShowLineBreak", typeof(bool), typeof(Config), new PropertyMetadata(true));

        /// <summary>
        /// ドキュメントの表示に使用するフォント名
        /// </summary>
        public string FontName
        {
            get { return (string)GetValue(FontNameProperty); }
            set { SetValue(FontNameProperty, value); }
        }
        public static readonly DependencyProperty FontNameProperty =
            DependencyProperty.Register("FontName", typeof(string), typeof(Config), new PropertyMetadata(""));

        /// <summary>
        /// FooGrep.Exeへのパスを表す
        /// </summary>
        public string GrepPath
        {
            get { return (string)GetValue(GrepPathProperty); }
            set { SetValue(GrepPathProperty, value); }
        }
        public static readonly DependencyProperty GrepPathProperty =
            DependencyProperty.Register("GrepPath", typeof(string), typeof(Config), new PropertyMetadata(""));

        /// <summary>
        /// 文字列の表示に使用するアンチエイリアスモード
        /// </summary>
        public TextAntialiasMode TextAntialiasMode
        {
            get { return (TextAntialiasMode)GetValue(TextAntialiasModeProperty); }
            set { SetValue(TextAntialiasModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextAntialiasMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextAntialiasModeProperty =
            DependencyProperty.Register("TextAntialiasMode", typeof(TextAntialiasMode), typeof(Config), new PropertyMetadata(TextAntialiasMode.Default));
        
        /// <summary>
        /// 印刷時のヘッダー
        /// </summary>
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(Config), new PropertyMetadata(""));

        /// <summary>
        /// 印刷時のフッター
        /// </summary>
        public string Footer
        {
            get { return (string)GetValue(FooterProperty); }
            set { SetValue(FooterProperty, value); }
        }
        public static readonly DependencyProperty FooterProperty =
            DependencyProperty.Register("Footer", typeof(string), typeof(Config), new PropertyMetadata(""));

        /// <summary>
        /// ドキュメントの表示に使用するフォントサイズ
        /// </summary>
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(Config), new PropertyMetadata(0.0,ValidateUnSignedDoubleNumber));

        /// <summary>
        /// メインウィンドウの高さ
        /// </summary>
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(Config), new PropertyMetadata(0.0));

        /// <summary>
        /// メインウィンドウの幅
        /// </summary>
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(Config), new PropertyMetadata(0.0));

        /// <summary>
        /// 印刷時に使用する上余白
        /// </summary>
        public double TopSpace
        {
            get { return (double)GetValue(TopSpaceProperty); }
            set { SetValue(TopSpaceProperty, value); }
        }
        public static readonly DependencyProperty TopSpaceProperty =
            DependencyProperty.Register("TopSpace", typeof(double), typeof(Config), new PropertyMetadata(0.0,ValidateUnSignedDoubleNumber));

        /// <summary>
        /// 印刷時に使用する右余白
        /// </summary>
        public double RightSpace
        {
            get { return (double)GetValue(RightSpaceProperty); }
            set { SetValue(RightSpaceProperty, value); }
        }
        public static readonly DependencyProperty RightSpaceProperty =
            DependencyProperty.Register("RightSpace", typeof(double), typeof(Config), new PropertyMetadata(0.0, ValidateUnSignedDoubleNumber));

        /// <summary>
        /// 印刷時に使用する下余白
        /// </summary>
        public double BottomSpace
        {
            get { return (double)GetValue(BottomSpaceProperty); }
            set { SetValue(BottomSpaceProperty, value); }
        }
        public static readonly DependencyProperty BottomSpaceProperty =
            DependencyProperty.Register("BottomSpace", typeof(double), typeof(Config), new PropertyMetadata(0.0, ValidateUnSignedDoubleNumber));

        /// <summary>
        /// 印刷時に使用する左余白
        /// </summary>
        public double LeftSpace
        {
            get { return (double)GetValue(LeftSpaceProperty); }
            set { SetValue(LeftSpaceProperty, value); }
        }
        public static readonly DependencyProperty LeftSpaceProperty =
            DependencyProperty.Register("LeftSpace", typeof(double), typeof(Config), new PropertyMetadata(0.0, ValidateUnSignedDoubleNumber));

        /// <summary>
        /// 最大世代数
        /// </summary>
        public int MaxBackupCount
        {
            get { return (int)GetValue(MaxBackupCountProperty); }
            set { SetValue(MaxBackupCountProperty, value); }
        }
        public static readonly DependencyProperty MaxBackupCountProperty =
            DependencyProperty.Register("MaxBackupCount", typeof(int), typeof(Config), new PropertyMetadata(0,ValidateUnSignedIntegerNumber));

        /// <summary>
        /// タブの幅を文字数で指定する
        /// </summary>
        public int TabStops
        {
            get { return (int)GetValue(TabStopsProperty); }
            set { SetValue(TabStopsProperty, value); }
        }
        public static readonly DependencyProperty TabStopsProperty =
            DependencyProperty.Register("TabStops", typeof(int), typeof(Config), new PropertyMetadata(0, ValidateUnSignedIntegerNumber));

        /// <summary>
        /// 自動保存を行う間隔（間隔は変更回数）
        /// </summary>
        public int AutoSaveCount
        {
            get { return (int)GetValue(AutoSaveCountProperty); }
            set { SetValue(AutoSaveCountProperty, value); }
        }
        public static readonly DependencyProperty AutoSaveCountProperty =
            DependencyProperty.Register("AutoSaveCount", typeof(int), typeof(Config), new PropertyMetadata(0, ValidateUnSignedIntegerNumber));

        /// <summary>
        /// 折り返しの方法を指定する
        /// </summary>
        public LineBreakMethod LineBreakMethod
        {
            get { return (LineBreakMethod)GetValue(LineBreakMethodProperty); }
            set { SetValue(LineBreakMethodProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineBreakMethod.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineBreakMethodProperty =
            DependencyProperty.Register("LineBreakMethod", typeof(LineBreakMethod), typeof(Config), new PropertyMetadata(LineBreakMethod.None));

        /// <summary>
        /// 折り返す桁数を指定する
        /// </summary>
        public int LineBreakCharCount
        {
            get { return (int)GetValue(LineBreakCharCountProperty); }
            set { SetValue(LineBreakCharCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineBreakCharCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineBreakCharCountProperty =
            DependencyProperty.Register("LineBreakCharCount", typeof(int), typeof(Config), new PropertyMetadata(80, ValidateUnSignedIntegerNumber));

        /// <summary>
        /// ルーラーを表示するなら真
        /// </summary>
        public bool DrawRuler
        {
            get { return (bool)GetValue(DrawRulerProperty); }
            set { SetValue(DrawRulerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawRuler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawRulerProperty =
            DependencyProperty.Register("DrawRuler", typeof(bool), typeof(Config), new PropertyMetadata(false));

        /// <summary>
        /// オートインデントを行うなら真
        /// </summary>
        public bool AutoIndent
        {
            get { return (bool)GetValue(AutoIndentProperty); }
            set { SetValue(AutoIndentProperty, value); }
        }
        public static readonly DependencyProperty AutoIndentProperty =
            DependencyProperty.Register("AutoIndent", typeof(bool), typeof(Config), new PropertyMetadata(false));

        /// <summary>
        /// 行番号を表示するなら真
        /// </summary>
        public bool DrawLineNumber
        {
            get { return (bool)GetValue(DrawLineNumberProperty); }
            set { SetValue(DrawLineNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawLineNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawLineNumberProperty =
            DependencyProperty.Register("DrawLineNumber", typeof(bool), typeof(Config), new PropertyMetadata(false));
        
        /// <summary>
        /// 現在行にマークを表示するなら真
        /// </summary>
        public bool DrawLine
        {
            get { return (bool)GetValue(DrawLineProperty); }
            set { SetValue(DrawLineProperty, value); }
        }
        public static readonly DependencyProperty DrawLineProperty =
            DependencyProperty.Register("DrawLine", typeof(bool), typeof(Config), new PropertyMetadata(false));

        /// <summary>
        /// URLを表示するなら真
        /// </summary>
        public bool UrlMark
        {
            get { return (bool)GetValue(UrlMarkProperty); }
            set { SetValue(UrlMarkProperty, value); }
        }
        public static readonly DependencyProperty UrlMarkProperty =
            DependencyProperty.Register("UrlMark", typeof(bool), typeof(Config), new PropertyMetadata(false));


        public bool IndentBySpace
        {
            get { return (bool)GetValue(IndentBySpaceProperty); }
            set { SetValue(IndentBySpaceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IndentBySpace.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndentBySpaceProperty =
            DependencyProperty.Register("IndentBySpace", typeof(bool), typeof(Config), new PropertyMetadata(false));

        
        /// <summary>
        /// 文字色
        /// </summary>
        public System.Windows.Media.Color Fore
        {
            get { return (System.Windows.Media.Color)GetValue(ForeProperty); }
            set { SetValue(ForeProperty, value); }
        }
        public static readonly DependencyProperty ForeProperty =
            DependencyProperty.Register("Fore", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(SystemColors.WindowTextColor));

        /// <summary>
        /// 背景色
        /// </summary>
        public System.Windows.Media.Color Back
        {
            get { return (System.Windows.Media.Color)GetValue(BackProperty); }
            set { SetValue(BackProperty, value); }
        }
        public static readonly DependencyProperty BackProperty =
            DependencyProperty.Register("Back", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(SystemColors.WindowColor));

        /// <summary>
        /// 選択時の背景色
        /// </summary>
        public System.Windows.Media.Color Hilight
        {
            get { return (System.Windows.Media.Color)GetValue(HilightProperty); }
            set { SetValue(HilightProperty, value); }
        }
        public static readonly DependencyProperty HilightProperty =
            DependencyProperty.Register("Hilight", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(SystemColors.HighlightColor));

        /// <summary>
        /// URLの色
        /// </summary>
        public System.Windows.Media.Color URL
        {
            get { return (System.Windows.Media.Color)GetValue(URLProperty); }
            set { SetValue(URLProperty, value); }
        }
        public static readonly DependencyProperty URLProperty =
            DependencyProperty.Register("URL", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Blue));

        /// <summary>
        /// Keyword1を表示するときの色
        /// </summary>
        public System.Windows.Media.Color Keyword1
        {
            get { return (System.Windows.Media.Color)GetValue(Keyword1Property); }
            set { SetValue(Keyword1Property, value); }
        }
        public static readonly DependencyProperty Keyword1Property =
            DependencyProperty.Register("Keyword1", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Blue));

        /// <summary>
        /// Keyword2を表示するときの色
        /// </summary>
        public System.Windows.Media.Color Keyword2
        {
            get { return (System.Windows.Media.Color)GetValue(Keyword2Property); }
            set { SetValue(Keyword2Property, value); }
        }
        public static readonly DependencyProperty Keyword2Property =
            DependencyProperty.Register("Keyword2", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Aqua));

        /// <summary>
        /// コントロールコードを表示するときの色
        /// </summary>
        public System.Windows.Media.Color Control
        {
            get { return (System.Windows.Media.Color)GetValue(ControlProperty); }
            set { SetValue(ControlProperty, value); }
        }
        public static readonly DependencyProperty ControlProperty =
            DependencyProperty.Register("Control", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Gray));

        /// <summary>
        /// 文字リテラルを表示するときの色
        /// </summary>
        public System.Windows.Media.Color Literal
        {
            get { return (System.Windows.Media.Color)GetValue(LiteralProperty); }
            set { SetValue(LiteralProperty, value); }
        }
        public static readonly DependencyProperty LiteralProperty =
            DependencyProperty.Register("Literal", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Brown));

        /// <summary>
        /// コメントを表示するときの色
        /// </summary>
        public System.Windows.Media.Color Comment
        {
            get { return (System.Windows.Media.Color)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Green));

        /// <summary>
        /// キャレットの色を表す（挿入モード時）
        /// </summary>
        public System.Windows.Media.Color InsetCaret
        {
            get { return (System.Windows.Media.Color)GetValue(InsetCaretProperty); }
            set { SetValue(InsetCaretProperty, value); }
        }
        public static readonly DependencyProperty InsetCaretProperty =
            DependencyProperty.Register("InsetCaret", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(SystemColors.WindowTextColor));

        /// <summary>
        /// キャレットの色を表す（上書きモード時）
        /// </summary>
        public System.Windows.Media.Color OverwriteCaret
        {
            get { return (System.Windows.Media.Color)GetValue(OverwriteCaretProperty); }
            set { SetValue(OverwriteCaretProperty, value); }
        }
        public static readonly DependencyProperty OverwriteCaretProperty =
            DependencyProperty.Register("OverwriteCaret", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(SystemColors.WindowTextColor));

        public System.Windows.Media.Color LineMarker
        {
            get { return (System.Windows.Media.Color)GetValue(LineMarkerProperty); }
            set { SetValue(LineMarkerProperty, value); }
        }
        public static readonly DependencyProperty LineMarkerProperty =
            DependencyProperty.Register("LineMarker", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Gray));


        public System.Windows.Media.Color FoundMarker
        {
            get { return (System.Windows.Media.Color)GetValue(FoundMarkerProperty); }
            set { SetValue(FoundMarkerProperty, value); }
        }
        public static readonly DependencyProperty FoundMarkerProperty =
            DependencyProperty.Register("FoundMarker", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.Gray));

        public System.Windows.Media.Color UpdateArea
        {
            get { return (System.Windows.Media.Color)GetValue(UpdateAreaProperty); }
            set { SetValue(UpdateAreaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpdateArea.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateAreaProperty =
            DependencyProperty.Register("UpdateArea", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.MediumSeaGreen));


        public System.Windows.Media.Color LineNumber
        {
            get { return (System.Windows.Media.Color)GetValue(LineNumberProperty); }
            set { SetValue(LineNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineNumberProperty =
            DependencyProperty.Register("LineNumber", typeof(System.Windows.Media.Color), typeof(Config), new PropertyMetadata(Colors.DarkGray));

        
        private Config()
        {
            this.RecentFile = new RecentFileCollection();
            this.DontLoadPlugins = new List<string>();
            this.SyntaxDefinitions = new DocumentTypeCollection();
        }

        /// <summary>
        /// インスタンスを取得する
        /// </summary>
        static public Config GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Config();
                _instance.Load();
            }
            return _instance;
        }

        /// <summary>
        /// AppDataを保存するフォルダーへのフルパス
        /// </summary>
        public static string ApplicationFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"FooEditor");
            }
        }

        /// <summary>
        /// 実行ファイルがあるフォルダーへのフルパス
        /// </summary>
        public static string ExecutablePath
        {
            get
            {
                return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            }
        }

        /// <summary>
        /// 最近履歴の最大数
        /// </summary>
        public int RecentMaxCount
        {
            get {
                return this.RecentFile.MaxCount;
            }
            set {
                if (value < 0)
                    throw new ArithmeticException(Properties.Resources.VaildateErrorOnUnsignedNumber);
                this.RecentFile.MaxCount = value;
            }
        }

        /// <summary>
        /// 最近履歴を表す
        /// </summary>
        public RecentFileCollection RecentFile
        {
            get;
            private set;
        }
        
        /// <summary>
        /// 文章タイプを定義するコレクション
        /// </summary>
        public DocumentTypeCollection SyntaxDefinitions
        {
            get;
            private set;
        }

        /// <summary>
        /// 読み込まないプラグインリスト
        /// </summary>
        public List<string> DontLoadPlugins
        {
            get;
            private set;
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        public void Load()
        {
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey(RegAppPath);

            if (!this.IsVaildVersion(regkey))
            {
                regkey.Close();
                Registry.CurrentUser.DeleteSubKeyTree(RegAppPath);
                regkey = Registry.CurrentUser.CreateSubKey(RegAppPath);
            }

            this.ShowFoundPattern = bool.Parse((string)regkey.GetValue("ShowFoundPattern", "false"));
            this.FontName = (string)regkey.GetValue("FontName", Properties.Resources.DefaultTextBoxFontName);
            this.FontSize = double.Parse((string)regkey.GetValue("FontSize", SystemFonts.MessageFontSize.ToString()));
            this.Height = double.Parse((string)regkey.GetValue("Height","480"));
            this.Width = double.Parse((string)regkey.GetValue("Width", "640"));
            this.MaxBackupCount = (int)regkey.GetValue("MaxBackupCount", 4);
            this.RecentMaxCount = (int)regkey.GetValue("RecentCount", 4);
            this.DrawLine = bool.Parse((string)regkey.GetValue("DrawLine", "true"));
            this.UrlMark = bool.Parse((string)regkey.GetValue("UrlMark", "false"));
            this.AutoSaveCount = (int)regkey.GetValue("AutoSaveCount", 0);
            this.DrawLineNumber = bool.Parse((string)regkey.GetValue("DrawLineNumber", "false"));
            this.DrawRuler = bool.Parse((string)regkey.GetValue("DrawRuler", "false"));
            this.AutoIndent = bool.Parse((string)regkey.GetValue("AutoIndent", "false"));
            this.ShowHalfSpace = bool.Parse((string)regkey.GetValue("ShowHalfSpace", "false"));
            this.ShowFullSpace = bool.Parse((string)regkey.GetValue("ShowFullSpace", "true"));
            this.ShowTab = bool.Parse((string)regkey.GetValue("ShowTab", "true"));
            this.ShowLineBreak = bool.Parse((string)regkey.GetValue("ShowLineBreak", "true"));
            this.LineBreakMethod = (LineBreakMethod)(int)regkey.GetValue("LineBreakMethod", (int)LineBreakMethod.None);
            this.LineBreakCharCount = (int)regkey.GetValue("LineBreakCharCount", 80);
            string[] temps = (string[])regkey.GetValue("RecentFile", new string[0]);
            foreach (string temp in temps)
                this.RecentFile.Add(temp);
            temps = (string[])regkey.GetValue("DontLoadPlugin", new string[0]);
            this.DontLoadPlugins.AddRange(temps);
            this.TabStops = (int)regkey.GetValue("TabStops", 4);
            this.IndentBySpace = bool.Parse((string)regkey.GetValue("IndentBySpace", "false"));
            this.GrepPath = (string)regkey.GetValue("GrepPath", Config.ExecutablePath + "\\FooGrep.EXE");
            this.Fore = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("ForeColor", ForeProperty.DefaultMetadata.DefaultValue.ToString()));
            this.Back = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("BackColor", BackProperty.DefaultMetadata.DefaultValue.ToString()));
            this.Comment = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("CommentColor", CommentProperty.DefaultMetadata.DefaultValue.ToString()));
            this.Keyword1 = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("Keyword1Color", Keyword1Property.DefaultMetadata.DefaultValue.ToString()));
            this.Keyword2 = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("Keyword2Color", Keyword2Property.DefaultMetadata.DefaultValue.ToString()));
            this.Literal = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("LiteralColor", LiteralProperty.DefaultMetadata.DefaultValue.ToString()));
            this.Control = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("ControlColor", ControlProperty.DefaultMetadata.DefaultValue.ToString()));
            this.UpdateArea = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("UpdateAreaColor", UpdateAreaProperty.DefaultMetadata.DefaultValue.ToString()));
            this.LineNumber = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("LineNumberColor", LineNumberProperty.DefaultMetadata.DefaultValue.ToString()));
            System.Windows.Media.Color DefaultFoundMarker = Colors.Gray;
            DefaultFoundMarker.A = 64;
            this.FoundMarker = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("FoundMarker", DefaultFoundMarker.ToString()));
            this.URL = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("URLColor", URLProperty.DefaultMetadata.DefaultValue.ToString()));
            System.Windows.Media.Color DefalutHilight = SystemColors.HighlightColor;
            DefalutHilight.A = 128;
            this.Hilight = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("HilightColor", DefalutHilight.ToString()));
            this.InsetCaret = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("InsetCaretColor", InsetCaretProperty.DefaultMetadata.DefaultValue.ToString()));
            System.Windows.Media.Color DefalutOverwriteCaret = SystemColors.WindowTextColor;
            DefalutOverwriteCaret.A = 128;
            this.OverwriteCaret = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("OverwriteCaretColor", DefalutOverwriteCaret.ToString()));
            System.Windows.Media.Color DefalutLineMarker = SystemColors.WindowTextColor;
            DefalutLineMarker.A = 64;
            this.LineMarker = (System.Windows.Media.Color)ColorConverter.ConvertFromString((string)regkey.GetValue("LineMarkerColor", DefalutLineMarker.ToString()));
            this.TopSpace = double.Parse((string)regkey.GetValue("TopSpace", "0"));
            this.RightSpace = double.Parse((string)regkey.GetValue("RightSpace", "0"));
            this.BottomSpace = double.Parse((string)regkey.GetValue("BottomSpace", "0"));
            this.LeftSpace = double.Parse((string)regkey.GetValue("LeftSpace", "0"));
            this.Header = (string)regkey.GetValue("Header", "%f");
            this.Footer = (string)regkey.GetValue("Fotter", "-%p-");
            this.TextAntialiasMode = (TextAntialiasMode)regkey.GetValue("TextAntialiasMode", (int)TextAntialiasMode.Default);
            string[] findHistory = (string[])regkey.GetValue("FindHistory",new string[0]);
            foreach (string s in findHistory)
                this._history.Add(s);
            this.DefaultEncoding = (string)regkey.GetValue("DefaultEncoding", System.Text.Encoding.Default.WebName);
            regkey.Close();

            this.SyntaxDefinitions.Clear();
            regkey = Registry.CurrentUser.OpenSubKey(SyntaxDefinitionPath);
            if (regkey == null)
            {
                this.SyntaxDefinitions.Add(new DocumentType(Resources.DocumetTypeNone));
                SetDefalutSytax();
                this.SyntaxDefinitions.Select(Resources.DocumetTypeNone);
                return;
            }
            foreach (string name in regkey.GetSubKeyNames())
            {
                RegistryKey childRegkey = regkey.OpenSubKey(name);
                DocumentType type = new DocumentType(name);
                type.NoInherit = bool.Parse((string)childRegkey.GetValue("NoInherit"));
                type.Extension = (string)childRegkey.GetValue("Extension");
                type.LineBreakMethod = (LineBreakMethod)(int)childRegkey.GetValue("LineBreakMethod");
                type.LineBreakCharCount = (int)childRegkey.GetValue("LineBreakCharCount");
                type.IsDrawLineNumber = bool.Parse((string)childRegkey.GetValue("DrawLineNumber"));
                type.IsAutoIndent = bool.Parse((string)childRegkey.GetValue("AutoIndent"));
                type.IsDrawRuler = bool.Parse((string)childRegkey.GetValue("DrawRuler"));
                type.ShowHalfSpace = bool.Parse((string)regkey.GetValue("ShowHalfSpace", "false"));
                type.ShowFullSpace = bool.Parse((string)regkey.GetValue("ShowFullSpace", "true"));
                type.ShowTab = bool.Parse((string)regkey.GetValue("ShowTab", "true"));
                type.ShowLineBreak = bool.Parse((string)regkey.GetValue("ShowLineBreak", "true"));
                type.IndentBySpace = bool.Parse((string)regkey.GetValue("IndentBySpace", "false"));
                type.TabStops = (int)childRegkey.GetValue("TabStops", 4);
                this.SyntaxDefinitions.Add(type);
                childRegkey.Close();
            }
            DocumentType noneType = this.SyntaxDefinitions.Find(Resources.DocumetTypeNone);
            if (noneType == null)
                this.SyntaxDefinitions.Add(new DocumentType(Resources.DocumetTypeNone, string.Empty));
            this.SyntaxDefinitions.Select(Resources.DocumetTypeNone);
            regkey.Close();

            return;
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public void Save()
        {
            RegistryKey regkey = Registry.CurrentUser.CreateSubKey(RegAppPath);
            regkey.SetValue("Version", ConfigVersion);
            regkey.SetValue("ShowFoundPattern", this.ShowFoundPattern);
            regkey.SetValue("FontName", this.FontName);
            regkey.SetValue("FontSize", this.FontSize);
            regkey.SetValue("Height", this.Height);
            regkey.SetValue("Width", this.Width);
            regkey.SetValue("MaxBackupCount", this.MaxBackupCount);
            regkey.SetValue("RecentCount", this.RecentMaxCount);
            regkey.SetValue("LineBreakMethod", (int)this.LineBreakMethod);
            regkey.SetValue("LineBreakCharCount", this.LineBreakCharCount);
            regkey.SetValue("DrawRuler", this.DrawRuler);
            regkey.SetValue("AutoIndent", this.AutoIndent);
            regkey.SetValue("DrawLine", this.DrawLine);
            regkey.SetValue("RecentFile", this.RecentFile.ToArray());
            regkey.SetValue("FindHistory", this._history.ToArray());
            regkey.SetValue("DontLoadPlugin", this.DontLoadPlugins.ToArray());
            regkey.SetValue("TabStops", this.TabStops);
            regkey.SetValue("UrlMark", this.UrlMark);
            regkey.SetValue("GrepPath", this.GrepPath);
            regkey.SetValue("AutoSaveCount", this.AutoSaveCount);
            regkey.SetValue("ForeColor", this.Fore.ToString());
            regkey.SetValue("BackColor", this.Back.ToString());
            regkey.SetValue("CommentColor", this.Comment.ToString());
            regkey.SetValue("Keyword1Color", this.Keyword1.ToString());
            regkey.SetValue("Keyword2Color", this.Keyword2.ToString());
            regkey.SetValue("LiteralColor", this.Literal.ToString());
            regkey.SetValue("ControlColor", this.Control.ToString());
            regkey.SetValue("InsetCaretColor", this.InsetCaret.ToString());
            regkey.SetValue("OverwriteCaretColor", this.OverwriteCaret.ToString());
            regkey.SetValue("LineMarkerColor", this.LineMarker.ToString());
            regkey.SetValue("FoundMarker", this.FoundMarker.ToString());
            regkey.SetValue("URLColor", this.URL.ToString());
            regkey.SetValue("HilightColor", this.Hilight.ToString());
            regkey.SetValue("UpdateAreaColor",this.UpdateArea.ToString());
            regkey.SetValue("TopSpace", this.TopSpace);
            regkey.SetValue("RightSpace", this.RightSpace);
            regkey.SetValue("BottomSpace", this.BottomSpace);
            regkey.SetValue("LeftSpace", this.LeftSpace);
            regkey.SetValue("Header", this.Header);
            regkey.SetValue("Fotter", this.Footer);
            regkey.SetValue("TextAntialiasMode", (int)this.TextAntialiasMode);
            regkey.SetValue("ShowHalfSpace", this.ShowHalfSpace);
            regkey.SetValue("ShowFullSpace", this.ShowFullSpace);
            regkey.SetValue("ShowTab", this.ShowTab);
            regkey.SetValue("ShowLineBreak", this.ShowLineBreak);
            regkey.SetValue("IndentBySpace", this.IndentBySpace);
            regkey.SetValue("LineNumberColor", this.LineNumber.ToString());
            regkey.SetValue("DefaultEncoding", this.DefaultEncoding);
            regkey.Close();

            regkey =  Registry.CurrentUser.CreateSubKey(SyntaxDefinitionPath);
            foreach (DocumentType item in SyntaxDefinitions)
            {
                if (item.Extension != null)
                {
                    RegistryKey childRegkey = regkey.CreateSubKey(item.Name);
                    childRegkey.SetValue("NoInherit", item.NoInherit);
                    childRegkey.SetValue("LineBreakMethod", (int)item.LineBreakMethod);
                    childRegkey.SetValue("LineBreakCharCount", item.LineBreakCharCount);
                    childRegkey.SetValue("DrawRuler", item.IsDrawRuler);
                    childRegkey.SetValue("DrawLineNumber", item.IsDrawLineNumber);
                    childRegkey.SetValue("AutoIndent", item.IsAutoIndent);
                    childRegkey.SetValue("Extension", item.Extension);
                    childRegkey.SetValue("ShowHalfSpace", item.ShowHalfSpace);
                    childRegkey.SetValue("ShowFullSpace", item.ShowFullSpace);
                    childRegkey.SetValue("ShowTab", item.ShowTab);
                    childRegkey.SetValue("ShowLineBreak", item.ShowLineBreak);
                    childRegkey.SetValue("IndentBySpace", item.IndentBySpace);
                    childRegkey.SetValue("TabStops", item.TabStops);
                    childRegkey.Close();
                }
            }
            regkey.Close();            
        }

        private bool IsVaildVersion(RegistryKey regkey)
        {
            string version = (string)regkey.GetValue("Version");
            if (version == null)
                return false;
            return version == ConfigVersion;
        }

        private void SetDefalutSytax()
        {
            Tuple<string, string>[] extensions = new Tuple<string, string>[]{
                new Tuple<string,string>("clang.xml",".+\\.(c|cpp|cs)$"),
                new Tuple<string,string>("css.xml",".+\\.(css)$"),
                new Tuple<string,string>("html.xml",".+\\.(htm|html|xhtml)$"),
                new Tuple<string,string>("javascript.xml",".+\\.(js)$"),
                new Tuple<string,string>("coffeescript.xml",".+\\.(coffee)$"),
                new Tuple<string,string>("php.xml", ".+\\.(php)$"),
                new Tuple<string,string>("python.xml",".+\\.(py)$"),
                new Tuple<string,string>("ruby.xml",".+\\.(rb)$"),
                new Tuple<string,string>("perl.xml",".+\\.(vb|vbs)$"),
                new Tuple<string,string>("java.xml",".+\\.(pl)$"),
                new Tuple<string,string>("xml.xml",".+\\.(java)$")
            };
            foreach (Tuple<string,string> ext in extensions)
            {
                this.SyntaxDefinitions.Add(new DocumentType(ext.Item1,ext.Item2));
            }
        }
    }
}
