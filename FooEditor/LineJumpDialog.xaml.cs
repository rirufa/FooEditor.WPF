using System.Windows;
using System.Windows.Controls;
using FooEditEngine.WPF;

namespace FooEditor
{
    /// <summary>
    /// LineJumpDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class LineJumpDialog : Window,ILineJumpView
    {
        FooTextBox Textbox;

        public LineJumpDialog(FooTextBox textbox)
            : this()
        {
            this.Textbox = textbox;
            this.Model = new LineJumpViewModel(this);
            this.DataContext = this.Model;
        }
        /// <summary>
        /// コンストラクター
        /// </summary>
        public LineJumpDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        LineJumpViewModel Model;

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var hasErros = Validation.GetErrors(this.JumpToTextBox);
            if (hasErros.Count == 0)
            {
                this.Model.JumpCaretCommand();
                this.Close();
            }
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public int CaretPostionRow
        {
            get { return this.Textbox.CaretPostion.row; }
        }

        public int AvailableMaxRow
        {
            get { return this.Textbox.LayoutLineCollection.Count; }
        }

        public void JumpCaret(int row)
        {
            this.Textbox.JumpCaret(row, 0);
            this.Textbox.Refresh();
        }
    }
}
