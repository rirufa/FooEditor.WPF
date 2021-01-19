using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Outline
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        ObservableCollection<AnalyzePattern> NewPatterns;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ConfigWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ConfigWindow(ObservableCollection<AnalyzePattern> patterns)
            : this()
        {
            foreach (AnalyzePattern pattern in patterns)
            {
                Label label = new Label();
                label.Content = pattern.Type;
                label.Tag = string.Join(Environment.NewLine,pattern.Patterns);
                this.AnalyzeNameList.Items.Add(label);
            }
            this.NewPatterns = patterns;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.NewPatterns.Clear();
            foreach (Label label in this.AnalyzeNameList.Items)
            {
                string type = (string)label.Content;
                string patterns = (string)label.Tag;
                this.NewPatterns.Add(new AnalyzePattern(type, patterns.Split(new string[]{Environment.NewLine},StringSplitOptions.RemoveEmptyEntries)));
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddAnalyze_Click(object sender, RoutedEventArgs e)
        {
            Label label = new Label();
            label.Content = "NewItem";
            label.Tag = "";
            this.AnalyzeNameList.Items.Add(label);
            this.AnalyzeNameList.SelectedItem = label;
        }

        private void RemoveAnalyze_Click(object sender, RoutedEventArgs e)
        {
            int i;
            for (i = 0; i < this.AnalyzeNameList.Items.Count; i++)
            {
                Label label = (Label)this.AnalyzeNameList.Items[i];
                if ((string)label.Content == this.SyntaxFileName.Text)
                    break;
            }
            if (i < this.AnalyzeNameList.Items.Count)
                this.AnalyzeNameList.Items.RemoveAt(i);
        }
    }
}
