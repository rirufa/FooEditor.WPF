using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs.Controls;
using FooEditor.Properties;
using EncodeDetect;

namespace FooEditor
{
    abstract public class CustomFileDialogBase : IDisposable
    {
        protected SortedList<string, Encoding> list = new SortedList<string, Encoding>();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public CustomFileDialogBase()
        {
            foreach (EncodingInfo info in Encoding.GetEncodings())
            {
                var enc = info.GetEncoding();
                if (enc == UTF8Encoding.UTF8)
                    continue;
                if (list.ContainsKey(info.DisplayName) == false)
                    list.Add(info.DisplayName, enc);
            }
            UTF8WithBom utf8enc = new UTF8WithBom();
            list.Add("Unicode(UTF-8 BOM)", utf8enc);
            UTF8WithoutBom utf8nobom = new UTF8WithoutBom();
            list.Add("Unicode(UTF-8)", utf8nobom);
        }

        /// <summary>
        /// 選択されたファイル
        /// </summary>
        public string FileName
        {
            get;
            protected set;
        }

        /// <summary>
        /// 選択されたエンコーディング
        /// </summary>
        public Encoding FileEncoding
        {
            get;
            set;
        }

        /// <summary>
        /// ダイアログを開いた時に表示するフォルダーへのフルパス
        /// </summary>
        public abstract string InitialDirectory
        {
            set;
        }

        /// <summary>
        /// タイトル
        /// </summary>
        public abstract string Title
        {
            set;
        }

        /// <summary>
        /// オブジェクトを開放する
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// ダイアログを表示する
        /// </summary>
        /// <returns></returns>
        public abstract CommonFileDialogResult ShowDialog();

        /// <summary>
        /// フィルターを追加する
        /// </summary>
        /// <param name="show">タイトル</param>
        /// <param name="ext">拡張子</param>
        public abstract void addFilter(string show, string ext);
    }

    public sealed  class CustomOpenFileDialog : CustomFileDialogBase
    {
        CommonOpenFileDialog ofd;

        public override string InitialDirectory
        {
            set { ofd.InitialDirectory = value;}
        }

        public override string Title
        {
            set { ofd.Title = value; }
        }

        public CustomOpenFileDialog()
        {
            ofd = new CommonOpenFileDialog();
        }

        public override void addFilter(string show, string ext)
        {
            ofd.Filters.Add(new CommonFileDialogFilter(show,ext));
        }

        public override CommonFileDialogResult ShowDialog()
        {
            CommonFileDialogComboBox comboBox = new CommonFileDialogComboBox("comboBox1");
            int i = 0;
            foreach (var kv in list)
            {
                comboBox.Items.Add(new CommonFileDialogComboBoxItem(kv.Key));
                if (this.FileEncoding != null && kv.Value.WebName == this.FileEncoding.WebName)
                    comboBox.SelectedIndex = i;
                i++;
            }
            comboBox.Enabled = false;
            ofd.Controls.Add(new CommonFileDialogLabel(Resources.FileDialogCodepageLabel));
            ofd.Controls.Add(comboBox);

            CommonFileDialogCheckBox chekckBox = new CommonFileDialogCheckBox("CheckBox1");
            chekckBox.CheckedChanged += new EventHandler(chekckBox_CheckedChanged);
            chekckBox.Text = Resources.FileDialogCheckBox;
            chekckBox.IsChecked = true;
            ofd.Controls.Add(chekckBox);

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (chekckBox.IsChecked != true)
                {
                    this.FileEncoding = list[comboBox.Items[comboBox.SelectedIndex].Text];
                }
                else
                {
                    this.FileEncoding = null;
                }
                this.FileName = ofd.FileName;
                return CommonFileDialogResult.Ok;
            }
            else
                return CommonFileDialogResult.Cancel;
        }

        public override void Dispose()
        {
            ofd.Dispose();
        }

        void chekckBox_CheckedChanged(object sender, EventArgs e)
        {
            CommonFileDialogCheckBox checkbox = (CommonFileDialogCheckBox)sender;
            ofd.Controls["comboBox1"].Enabled = !checkbox.IsChecked;
        }
    }

    public sealed class CustomSaveFileDialog : CustomFileDialogBase
    {
        CommonSaveFileDialog sfd;

        public override string InitialDirectory
        {
            set { sfd.InitialDirectory = value; }
        }

        public override string Title
        {
            set { sfd.Title = value; }
        }

        public LineFeedType LineFeed
        {
            get;
            set;
        }

        public CustomSaveFileDialog()
        {
            sfd = new CommonSaveFileDialog();
        }

        public override void addFilter(string show, string ext)
        {
            sfd.Filters.Add(new CommonFileDialogFilter(show,ext));
        }

        public override CommonFileDialogResult ShowDialog()
        {
            CommonFileDialogComboBox comboBox = new CommonFileDialogComboBox("comboBox1");
            int i = 0;
            foreach (var kv in list)
            {
                comboBox.Items.Add(new CommonFileDialogComboBoxItem(kv.Key));
                if (kv.Value.WebName == this.FileEncoding.WebName)
                    comboBox.SelectedIndex = i;
                i++;
            }
            sfd.Controls.Add(new CommonFileDialogLabel(Resources.FileDialogCodepageLabel));
            sfd.Controls.Add(comboBox);

            LineFeedConverter lfconv = new LineFeedConverter();
            LineFeedType[] lfs = new LineFeedType[] { LineFeedType.CR, LineFeedType.LF, LineFeedType.CRLF };
            CommonFileDialogComboBox comboBox2 = new CommonFileDialogComboBox("LineFeedList");
            for (i = 0; i < lfs.Length; i++)
            {
                string text = (string)lfconv.Convert(lfs[i], typeof(LineFeedType), null, null);
                comboBox2.Items.Add(new CommonFileDialogComboBoxItem(text));
                if (lfs[i] == this.LineFeed)
                    comboBox2.SelectedIndex = i;
            }
            sfd.Controls.Add(new CommonFileDialogLabel(Resources.FileDialogLineFeedLable));
            sfd.Controls.Add(comboBox2);

            if (sfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.FileEncoding = list[comboBox.Items[comboBox.SelectedIndex].Text];
                this.LineFeed = (LineFeedType)lfconv.ConvertBack(comboBox2.Items[comboBox2.SelectedIndex].Text, typeof(string), null, null);
                this.FileName = sfd.FileName;
                return CommonFileDialogResult.Ok;
            }
            else
                return CommonFileDialogResult.Cancel;
        }

        public override void Dispose()
        {
            sfd.Dispose();
        }
    }
}
