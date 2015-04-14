using System;
using System.Windows.Data;
using System.Windows.Controls;

namespace Client.Helpers
{
    public class PasswordTuple : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (object)new Tuple<PasswordBox, PasswordBox>((PasswordBox)values[0], (PasswordBox)values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
