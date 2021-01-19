using System.Windows;

namespace AutoComplete
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        /// <summary>
        /// コンストラクター
        /// </summary>
        public ConfigWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ConfigWindow_Loaded);
        }

        void ConfigWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.CreateFromDoc.IsChecked = this.CollectWords;
        }

        /// <summary>
        /// 補完候補を自動収集するなら真
        /// </summary>
        public bool CollectWords
        {
            get;
            set;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.CollectWords = (bool)this.CreateFromDoc.IsChecked;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
  
}
