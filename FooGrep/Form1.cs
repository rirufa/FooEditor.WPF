using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FooGrep.Properties;

namespace FooGrep
{
    public partial class Form1 : Form
    {
        Dictionary<string, int> NameToCodePage;
        CancellationTokenSource tokenSource;
        Task task;

        public Form1()
        {
            InitializeComponent();
            this.NameToCodePage = new Dictionary<string, int>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                this.textBox1.Text = fbd.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.textBox2.Text == string.Empty)
                return;
            
            string dir,wildcard;
            SpilitPathAndFilePart(this.textBox1.Text,out dir,out wildcard);
            
            RegexOptions opt = this.checkBox3.Checked ? RegexOptions.IgnoreCase : RegexOptions.None;

            Encoding enc = null;
            if (this.checkBox4.Checked == false)
                enc = Encoding.GetEncoding(this.NameToCodePage[(string)this.comboBox1.SelectedItem]);

            ExecTask(this.listView1, dir, wildcard, this.checkBox1.Checked, (s,t) =>
            {
                try
                {
                    foreach (DocumentMatch m in Document.Find(s, enc, this.textBox2.Text, this.checkBox2.Checked, opt))
                    {
                        Invoke(new Action(() =>
                        {
                            string[] item = { s, m.lineNumber.ToString(), m.foundString };
                            this.listView1.Items.Add(new ListViewItem(item));
                        }));
                        if (t.IsCancellationRequested)
                            t.ThrowIfCancellationRequested();
                    }
                }
                catch (IOException)
                {
                }
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (EncodingInfo enc in Encoding.GetEncodings())
            {
                this.comboBox1.Items.Add(enc.DisplayName);
                if (!NameToCodePage.ContainsKey(enc.DisplayName))
                    this.NameToCodePage.Add(enc.DisplayName, enc.CodePage);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();

            EnableUIElements();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.textBox2.Text == string.Empty || this.textBox3.Text == string.Empty)
                return;

            string dir, wildcard;
            SpilitPathAndFilePart(this.textBox1.Text, out dir, out wildcard);

            RegexOptions opt = this.checkBox3.Checked ? RegexOptions.IgnoreCase : RegexOptions.None;

            Encoding enc = null;
            if (this.checkBox4.Checked == false)
                enc = Encoding.GetEncoding(this.NameToCodePage[(string)this.comboBox1.SelectedItem]);

            ExecTask(this.listView1, dir, wildcard, this.checkBox1.Checked, (s, t) =>
            {
                try
                {
                    Document.ReplaceAll(s, enc, this.textBox2.Text, this.textBox3.Text, this.checkBox2.Checked,opt,this.checkBox5.Checked);
                    Invoke(new Action(() =>
                    {
                        string[] item = { s, string.Empty,Resources.ReplaceComplete };
                        this.listView1.Items.Add(new ListViewItem(item));
                    }));
                    if (t.IsCancellationRequested)
                        t.ThrowIfCancellationRequested();
                }
                catch (IOException)
                {
                }
            });
        }

        void EnableUIElements()
        {
            this.button2.Enabled = true;
            this.button3.Enabled = false;
            this.button4.Enabled = true;
        }

        void DisableUIElements()
        {
            this.button2.Enabled = false;
            this.button3.Enabled = true;
            this.button4.Enabled = false;
        }

        void SpilitPathAndFilePart(string p, out string path, out string file)
        {
            path = Path.GetDirectoryName(p);

            file = Path.GetFileName(p);
        }

        void ExecTask(ListView listview, string dir, string wildcard,bool isRecursive,Action<string,CancellationToken> act)
        {
            DisableUIElements();

            if (this.task != null && this.task.IsCanceled)
            {
                this.task.Dispose();
                this.tokenSource.Dispose();
            }

            listview.Items.Clear();

            this.tokenSource = new CancellationTokenSource();

            this.task = Task.Factory.StartNew(() =>
            {
                try
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(dir, wildcard, isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                    foreach (string file in files)
                    {
                        this.tokenSource.Token.ThrowIfCancellationRequested();
                        act(file,this.tokenSource.Token);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }, this.tokenSource.Token);

            this.task.ContinueWith((e) =>
            {
                Invoke(new Action(() =>
                {
                    EnableUIElements();
                }));
            });
        }
    }
}
