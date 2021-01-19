using System;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace FooEditor
{
    sealed class PipeCommandListener
    {
        MainWindow form;

        public PipeCommandListener(MainWindow form)
        {
            this.form = form;
        }

        public void Execute(string data)
        {
            string[] cmd = data.Split('\t');

            switch (cmd[0])
            {
                case "NEWDOC":
                    form.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DocumentWindow document = form.CreateDocument();
                        form.ActivateDocument(document);
                    }), null);
                    break;
                case "OPEN":
                    if (cmd.Length >= 2)
                    {
                        form.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            DocumentWindow document = form.CreateDocument();
                            await document.LoadAsync(cmd[1], null);
                            form.ActivateDocument(document);
                        }), null);
                    }
                    break;
                case "OPENWITHLINEJUMP":
                    if (cmd.Length >= 3)
                    {
                        form.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            DocumentWindow document = form.CreateDocument();
                            await document.LoadAsync(cmd[1], null);
                            form.ActivateDocument(document);
                            int lineno = Int32.Parse(cmd[2]);
                            if (lineno > document.TextBox.LayoutLineCollection.Count)
                                return;
                            document.TextBox.JumpCaret(lineno, 0);
                            document.TextBox.Refresh();
                        }), null);
                    }
                    break;
                case "LINEJUMP":
                    if (cmd.Length >= 2)
                    {
                        form.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DocumentWindow document = form.DockManager.ActiveContent as DocumentWindow;
                            if (document == null)
                                return;
                            int lineno = Int32.Parse(cmd[1]);
                            if (lineno > document.TextBox.LayoutLineCollection.Count)
                                return;
                            document.TextBox.JumpCaret(lineno, 0);
                            document.TextBox.Refresh();
                        }), null);
                    }
                    break;
                case "ACTIVE":
                    form.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        form.Activate();
                    }), null);
                    break;
            }
        }
    }
}
