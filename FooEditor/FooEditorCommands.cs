using System;
using System.Windows.Input;

namespace FooEditor
{
    public static class FooEditorCommands
    {
        public static RoutedCommand SelectDocumentType = new RoutedCommand("SelectDocumentType", typeof(MainWindow));
        public static RoutedCommand OpenRecentFile = new RoutedCommand("OpenRecentFile", typeof(MainWindow));
        public static RoutedCommand Grep = new RoutedCommand("Grep", typeof(MainWindow));
        public static RoutedCommand Quit = new RoutedCommand("Quit", typeof(MainWindow));
        public static RoutedCommand SaveWorkSpace = new RoutedCommand("SaveWorkSpace", typeof(MainWindow));
        public static RoutedCommand About = new RoutedCommand("About", typeof(MainWindow));
        public static RoutedCommand LineJump = new RoutedCommand("LineJump",typeof(MainWindow), 
            new InputGestureCollection() { new KeyGesture(Key.G, ModifierKeys.Control) });
        public static RoutedCommand GenerateFolding = new RoutedCommand("GenerateFolding", typeof(MainWindow));
    }
}
