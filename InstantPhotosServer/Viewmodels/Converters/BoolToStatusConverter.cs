using System;
using System.Globalization;
using System.Windows.Data;

namespace InstantPhotosServer
{
    class BoolToStatusConverter : IValueConverter
    {
        /// <summary>
        /// Converts boolean value to status string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == false ? "Offline" : "Online";
        }

        /// <summary>
        /// Converts status string to boolean value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == "Online" ? true : false;
        }
    }
}
