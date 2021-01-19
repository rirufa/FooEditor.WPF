using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using FooEditor.Plugin;

namespace FooEditor
{
    sealed class PanesStyleSelector : StyleSelector
    {
        public Style ToolStyle
        {
            get;
            set;
        }

        public Style DocumentStyle
        {
            get;
            set;
        }

        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is IToolWindow)
                return ToolStyle;

            if (item is DocumentWindow)
                return DocumentStyle;

            return base.SelectStyle(item, container);
        }
    }
}
