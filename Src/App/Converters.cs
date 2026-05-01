using LocalWinAI.Domain;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace LocalWinAI;

public class SenderToColumnConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (ChatMessageSender)value == ChatMessageSender.User ? 1 : 0;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class SenderToAlignmentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (ChatMessageSender)value == ChatMessageSender.User ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                    hex = "FF" + hex;
                var a = System.Convert.ToByte(hex[0..2], 16);
                var r = System.Convert.ToByte(hex[2..4], 16);
                var g = System.Convert.ToByte(hex[4..6], 16);
                var b = System.Convert.ToByte(hex[6..8], 16);
                return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            }
            catch { }
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
