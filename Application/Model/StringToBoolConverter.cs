using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FederalProjectManager.ORM;
using System.Windows.Markup;
using System.Globalization;
using FederalProjectManager.Model;
using SmartClasses;

namespace FederalProjectManager.Model
{
    public class StringToBoolConverter : MarkupExtension, IValueConverter
    {
        public StringToBoolConverter()
        {
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null) ? !String.IsNullOrEmpty(value.ToString()) : false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null) ? !String.IsNullOrEmpty(value.ToString()) : false;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
