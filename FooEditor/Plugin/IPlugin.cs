using System;
using System.Windows;

namespace FooEditor.Plugin
{
    public interface IToolWindow
    {
        /// <summary>
        /// アクティブなら真を返し、そうでないなら偽を返す
        /// </summary>
        /// <remarks>
        /// 実装する場合は必ず依存プロパティの形で実装してください
        /// </remarks>
        bool IsActive
        {
            get;
            set;
        }
    }

    public class ExplorerBar<T> where T : IToolWindow
    {
        T _content;
        public ExplorerBar()
        {
        }
        public T Content
        {
            get
            {
                return this._content;
            }
            set
            {
                this._content = value;
            }
        }
        public bool IsVisible
        {
            get
            {
                return this._content.IsActive;
            }
            set
            {
                this._content.IsActive = value;
            }
        }
    }
    /// <summary>
    /// プラグインインターフェイス
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 起動時に呼び出されるメソッド
        /// </summary>
        /// <param name="e"></param>
        void Initalize(MainWindow e);
        /// <summary>
        /// 終了時に呼び出されるメソッド
        /// </summary>
        void ClosedApp();
        /// <summary>
        /// 設定ダイアログを表示するときに呼び出されるメソッド
        /// </summary>
        void ShowConfigForm();
    }
}
