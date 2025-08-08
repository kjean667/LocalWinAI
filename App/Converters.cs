using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace LocalWinAI
{
    public class SenderToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (ChatMessageSender)value == ChatMessageSender.User ? 1 : 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

    public class SenderToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (ChatMessageSender)value == ChatMessageSender.User ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

}
