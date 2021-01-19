using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using EncodeDetect;
using FooEditEngine;
using FooEditor.Properties;

namespace FooEditor
{
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class HilightTypeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string type = (string)value;
            if (type == null)
                return Resources.DocumetTypeNone;
            else
                return type;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(LineBreakMethod), typeof(string))]
    public sealed class LineBreakMethodConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                LineBreakMethod method = (LineBreakMethod)value;
                switch (method)
                {
                    case LineBreakMethod.None:
                        return Resources.LineBreakMethodNone;
                    case LineBreakMethod.CharUnit:
                        return Resources.LineBreakMethodCharCount;
                    case LineBreakMethod.PageBound:
                        return Resources.LineBreakMethodPageBound;
                }
            }catch(InvalidCastException){
                return string.Empty;
            }
            throw new ArgumentOutOfRangeException();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            if (name == Resources.LineBreakMethodNone)
                return LineBreakMethod.None;
            if (name == Resources.LineBreakMethodCharCount)
                return LineBreakMethod.CharUnit;
            if (name == Resources.LineBreakMethodPageBound)
                return LineBreakMethod.PageBound;
            throw new ArgumentOutOfRangeException();
        }
    }

    [ValueConversion(typeof(LineFeedType), typeof(string))]
    public sealed class LineFeedConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            LineFeedType type = (LineFeedType)value;
            switch (type)
            {
                case LineFeedType.CR:
                    return "CR";
                case LineFeedType.CRLF:
                    return "CRLF";
                case LineFeedType.LF:
                    return "LF";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;
            switch (str)
            {
                case "CR":
                    return LineFeedType.CR;
                case "LF":
                    return LineFeedType.LF;
                case "CRLF":
                    return LineFeedType.CRLF;
            }
            throw new ArgumentOutOfRangeException();
        }
    }

    [ValueConversion(typeof(TextPoint), typeof(string))]
    public sealed class TextPointConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            TextPoint tp = (TextPoint)value;
            return string.Format(Resources.TextPointFormat,tp.row + 1,tp.col + 1);
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(Encoding), typeof(string))]
    public sealed class EncodingConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            Encoding enc = (Encoding)value;
            return enc.EncodingName;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    [ValueConversion(typeof(double), typeof(string))]
    public sealed class RateConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)value;
            return string.Format(Resources.MagnificationPowerFormat,(int)(v * 100));
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
