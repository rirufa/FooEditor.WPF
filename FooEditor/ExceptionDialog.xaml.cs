using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace FooEditor
{
    /// <summary>
    /// ExceptionDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ExceptionDialog : Window
    {
        /// <summary>
        /// コンストラクター
        /// </summary>
        public ExceptionDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="Exception">表示したい例外</param>
        public ExceptionDialog(Exception Exception)
            :this()
        {
        }

        /// <summary>
        /// 表示する例外の内容
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        /// <summary>
        /// 表示する文字列
        /// </summary>
        public string Message
        {
            get
            {
                if (this.Exception == null)
                    return string.Empty;
                string value = this.Exception.Message + Environment.NewLine
                    + this.Exception.StackTrace + Environment.NewLine;
                if(this.Exception.InnerException != null)
                    value += "---------InnerException----------" + Environment.NewLine
                    + this.Exception.InnerException.Message + Environment.NewLine
                    + this.Exception.InnerException.StackTrace;
                return value;
            }
        }

        private void Button_Click_Continue(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Button_Click_Exit(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
    }
}
