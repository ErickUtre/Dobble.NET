using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DobbleGame.Utilidades
{
    public class CadenaVisibilidadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            if (value is string texto && string.IsNullOrEmpty(texto))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
